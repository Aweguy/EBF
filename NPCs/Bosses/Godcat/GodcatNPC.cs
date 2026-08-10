using EBF.EbfUtils;
using EBF.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
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
        protected AttackManager attackManager = new();
        protected ref float StateTimer => ref NPC.localAI[0];

        //Dodging
        private bool isDodging = false;
        private bool hasDodged = false; // Used to display dodging frames

        //Phases
        private const int PhaseDuration = 60 * 25; // How long the godcats stick around before summoning their vehicle
        private const int FinalPhaseDuration = 60 * 10; // How long the godcats stick around before finishing the fight
        protected ref float Phase => ref NPC.ai[0];
        private ref float PhaseTimer => ref NPC.ai[1];
        
        //Other
        protected virtual int DustType => 0;

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
            NPC.damage = NPC.GetContactDamage();
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
                if (NPC.frameCounter <= 7)
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

        public override void OnSpawn(IEntitySource source)
        {
            // Sync initial state
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
        }

        public override void AI()
        {
            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;
            
            var player = Main.player[NPC.target];
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
        protected void SpawnDust()
        {
            for (var i = 0; i < 20; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustType);
        }
        private void HandleStateChange()
        {
            StateTimer++;
            
            // Only server should handle state changes
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            
            if (StateTimer >= stateDurations[currentState])
            {
                StateTimer = 0;
                currentState = currentState != State.Idle ? State.Idle : GetNextAttackState();
                
                // Sync state change
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
        }
        private State GetNextAttackState() => stateDurations.ElementAt(attackManager.Next()).Key;
        private void HandleDodging()
        {
            isDodging = Main.GameUpdateCount % 60 > 10;
            if (!isDodging)
                return;
            
            var npcBox = NPC.Hitbox;
            foreach (var proj in Main.projectile)
                if (proj.active && proj.friendly && !proj.minion && npcBox.Intersects(proj.Hitbox))
                    hasDodged = true;
        }
        private void HandlePhaseStuff(Player player)
        {
            PhaseTimer++;
            
            // Only server should handle phase transitions
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            
            if (Phase < 2 && PhaseTimer > PhaseDuration && currentState == State.Idle)
            {
                //Poof away or head to the ground
                var groundPos = NPC.Bottom.ToGroundPosition(false);
                if (NPC.Distance(groundPos) < 1500)
                {
                    currentState = State.GoingTowardsGround;
                    PhaseTimer = 0;
                    
                    // Sync state change
                    NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                }
                else
                {
                    SpawnDust();
                    SummonVehicle(player);
                    NPC.active = false;
                    
                    // Sync despawn
                    NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                }
            }
            else if (Phase == 2 && PhaseTimer > FinalPhaseDuration)
            {
                SpawnDust();
                NPC.StrikeInstantKill();
                
                // Sync death
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }

            if (currentState == State.InGround && PhaseTimer > 120)
            {
                SummonVehicle(player);
                NPC.active = false;
                
                // Sync despawn
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
        }
        private void DropToGround()
        {
            NPC.velocity.Y = Math.Clamp(NPC.velocity.Y + 0.1f, 0f, 4f);

            // Check if we've reached or passed the ground this frame
            Vector2 groundPos = NPC.Bottom.ToGroundPosition(false);
            if (NPC.Bottom.Y >= groundPos.Y - 8f)
            {
                NPC.velocity.Y = 0f;
                NPC.Bottom = groundPos;
                currentState = State.InGround;
                PhaseTimer = 0;
            }

            // Fallback distance check from player in case ground detection fails somehow
            Player player = Main.player[NPC.target];
            if (NPC.Distance(player.Center) > 2500)
            {
                SummonVehicle(player);
                NPC.active = false;
            }
        }
        
        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write((byte)currentState);
            writer.Write(StateTimer);
            writer.Write(Phase);
            writer.Write(PhaseTimer);
            writer.Write(isDodging);
            writer.Write(hasDodged);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            var newState = (State)reader.ReadByte();
            var newStateTimer = reader.ReadSingle();
            var newPhase = reader.ReadSingle();
            var newPhaseTimer = reader.ReadSingle();
            var newIsDodging = reader.ReadBoolean();
            var newHasDodged = reader.ReadBoolean();
    
            // Only update if values actually changed to prevent desync
            if (currentState != newState || Math.Abs(StateTimer - newStateTimer) > 0.1f)
            {
                currentState = newState;
                StateTimer = newStateTimer;
                Phase = newPhase;
                PhaseTimer = newPhaseTimer;
                isDodging = newIsDodging;
                hasDodged = newHasDodged;
            }
        }
    }
}
