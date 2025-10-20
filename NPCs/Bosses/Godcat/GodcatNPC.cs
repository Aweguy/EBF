using EBF.EbfUtils;
using EBF.Items.Magic;
using EBF.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    public abstract class GodcatNPC : ModNPC
    {
        //Attacks
        protected enum State : byte { Idle, GoingTowardsGround, InGround, LightJudgmentWave, SeikenStorm, SeikenRing, DarkReturnBall, LightDiamondWalls, DarkBallStream }
        protected Dictionary<State, int> stateDurations;
        protected State currentState = State.Idle;
        protected ref float StateTimer => ref NPC.localAI[0];

        //Dodging
        private bool isDodging = false;
        private bool hasDodged = false; // Used to display dodging frames

        //Phases
        private const int PhaseDuration = 60 * 25; // How long the godcats stick around before summoning their vehicle
        private const int FinalPhaseDuration = 60 * 10; // How long the godcats stick around before finishing the fight
        protected ref float Phase => ref NPC.ai[0];
        private ref float PhaseTimer => ref NPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 16;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 46;
            NPC.damage = 70;
            NPC.defense = 9999;
            NPC.lifeMax = 999999;
            NPC.noGravity = true;

            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit52;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f; // Use all spawn slots to prevent random NPCs from spawning

            NPC.lavaImmune = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Fallen_Blood");
        }
        public override bool CanBeHitByNPC(NPC attacker) => !isDodging;
        public override bool? CanBeHitByProjectile(Projectile projectile) => !isDodging;
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; //Prevent ignoring boss attacks by taking damage from other sources.
            return true;
        }
        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }
        public override void FindFrame(int frameHeight)
        {
            // Dodging frames
            if (hasDodged)
            {
                NPC.frame.Y = Main.rand.Next(6, 8) * frameHeight;
                hasDodged = false;
                return;
            }

            //Burrowing into ground
            if (currentState == State.InGround)
            {
                NPC.frame.Y = ((int)NPC.frameCounter + 8) * frameHeight;
                if(NPC.frameCounter <= 7)
                {
                    NPC.frameCounter += 0.1f;
                }
                return;
            }

            // Idle frames
            NPC.frameCounter += 0.1f;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
            }
            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }
        public override void AI()
        {
            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;

            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                NPC.noGravity = false;
                return;
            }

            if (currentState == State.GoingTowardsGround)
                DropToGround();
            else if (currentState != State.InGround)
                Move(player);
            
            HandleDodging();
            HandleAttacks(player);
            HandleStateChange();
            HandlePhaseStuff(player);
        }
        public override void OnKill()
        {
            //Let the world know the boss is dead
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedGodcat, -1);
        }
        protected abstract void Move(Player player);
        protected abstract void HandleAttacks(Player player);
        protected abstract void SummonVehicle(Player player);
        protected abstract void SpawnDust();
        private void HandleStateChange()
        {
            StateTimer++;
            if (StateTimer >= stateDurations[currentState])
            {
                StateTimer = 0;
                var index = Main.rand.Next(3, stateDurations.Count);
                currentState = currentState == State.Idle ? stateDurations.ElementAt(index).Key : State.Idle;
            }
        }
        private void HandleDodging()
        {
            isDodging = Main.GameUpdateCount % 60 > 10;
            if (isDodging)
            {
                Rectangle npcBox = NPC.Hitbox;
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.friendly && !proj.minion && npcBox.Intersects(proj.Hitbox))
                    {
                        hasDodged = true;
                    }
                }
            }
        }
        private void HandlePhaseStuff(Player player)
        {
            PhaseTimer++;
            if (Phase < 2 && PhaseTimer > PhaseDuration && currentState == State.Idle)
            {
                //Poof away or head to the ground
                var groundPos = NPC.Bottom.ToGroundPosition(false);
                if(NPC.Distance(groundPos) < 2000)
                {
                    currentState = State.GoingTowardsGround;
                    PhaseTimer = 0;
                }
                else
                {
                    SpawnDust();
                    SummonVehicle(player);
                    NPC.active = false;
                }
            }
            else if (Phase == 2 && PhaseTimer > FinalPhaseDuration)
            {
                SpawnDust();
                NPC.StrikeInstantKill();
            }

            if(currentState == State.InGround && PhaseTimer > 120)
            {
                SummonVehicle(player);
                NPC.active = false;
            }
        }
        private void DropToGround()
        {
            NPC.velocity.Y = Math.Clamp(NPC.velocity.Y + 0.1f, 0f, 4f);

            Vector2 groundPos = NPC.Bottom.ToGroundPosition(false);
            if (NPC.Bottom.Distance(groundPos) < 8)
            {
                NPC.velocity.Y = 0;
                NPC.Bottom = groundPos;
                currentState = State.InGround;
                PhaseTimer = 0;
            }
        }
    }
}
