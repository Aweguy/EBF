using EBF.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    public abstract class Godcat_VehicleNPC : ModNPC
    {
        //Textures
        protected Texture2D currentTexture;
        protected Asset<Texture2D> idleTexture;
        protected Asset<Texture2D> attackTexture;
        protected float animationSpeed = 0.1f;

        //AI
        private bool isTransitioningOut = false;
        private bool hasSearchedForOther = false; // We search for the other vehicle in AI, because OnSpawn is called before both vehicles are done initializing.
        protected NPC otherVehicle = null; // Is used to reduce aggression when both vehicles are active, and is also used to change phase only once both are dead.
        protected enum State : byte { Idle, TurningBallCircle, TurningBallSpiral, CreatorThunderBall, CreatorHolyDeathray, LimitBreak, DestroyerBallBurst, DestroyerBreath, DestroyerHomingBall, DestroyerFireWheel }
        protected State currentState = State.Idle;
        protected Dictionary<State, int> stateDurations;
        protected AttackManager attackManager = new();
        protected bool IsAlone => otherVehicle == null || !otherVehicle.active;
        protected ref float StateTimer => ref NPC.localAI[0];
        protected ref float Phase => ref NPC.ai[0];

        //Anti-fleeing system
        private ref float FramesOverPunishDistance => ref NPC.localAI[1];
        private const int PunishingDistance = 780;
        private const int PunishFrameThreshold = 80;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetDefaults()
        {
            NPC.damage = NPC.GetContactDamage();
            NPC.defense = 50;
            NPC.lifeMax = 300000;
            NPC.noGravity = true;

            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f; // Use all spawn slots to prevent random NPCs from spawning

            NPC.lavaImmune = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Fallen_Blood");
        }
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
            // Idle frames
            NPC.frameCounter += animationSpeed;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type])
            {
                if (currentTexture != idleTexture.Value)
                    SetAnimation(idleTexture, 6);
                else
                    NPC.frameCounter = 0;
            }
            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Zenith deals significantly less damage
            if (projectile.type == ProjectileID.FinalFractal)
                modifiers.FinalDamage *= 0.1f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Spawn with half health in second phase
            if (Phase == 1)
            {
                NPC.life = NPC.lifeMax / 2;
            }
        }
        public override void AI()
        {
            // Locate other vehicle when both are alive at once
            if (Phase != 0 && !hasSearchedForOther && TryFindOtherVehicle(out NPC otherVehicle))
            {
                this.otherVehicle = otherVehicle;
                hasSearchedForOther = true;
            }

            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;
            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                NPC.noGravity = false;
                return;
            }

            // In first phase, leave at half health
            if (Phase == 0 && NPC.life <= NPC.lifeMax / 2 && !isTransitioningOut)
            {
                isTransitioningOut = true;
                currentState = State.Idle;
                StateTimer = 0;
                CreateHalfHealthHurtEffect();

                NPC.velocity = new Vector2(-NPC.direction * 0.1f, 0);
            }

            if (isTransitioningOut)
            {
                NPC.velocity.X *= 1.1f;
                if (NPC.Distance(player.Center) > 3000)
                {
                    BeginNextPhase(player);
                    NPC.active = false;
                    return;
                }
            }
            else
            {
                //Do normal behavior
                Move(player);
                HandleStateChange();
                PunishFleeingPlayer(player);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!idleTexture.IsLoaded)
                return false;

            var position = NPC.Center - screenPos;
            var origin = NPC.frame.Size() * 0.5f;
            var flipX = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(currentTexture, position, NPC.frame, drawColor, 0, origin, NPC.scale, flipX, 0);
            return false;
        }
        public override void OnKill()
        {
            //Go to next phase if both are dead
            if (IsAlone)
            {
                //Bring back both godcats
                var pos = Main.player[NPC.target].position.ToPoint() + new Point(-NPC.direction * 1600, 0);
                var type = ModContent.NPCType<Godcat_Light>();
                NPC.NewNPC(NPC.GetSource_Death(), pos.X, pos.Y, type, 0, 2);

                var pos2 = Main.player[NPC.target].position.ToPoint() + new Point(-NPC.direction * 1600, 0);
                var type2 = ModContent.NPCType<Godcat_Dark>();
                NPC.NewNPC(NPC.GetSource_Death(), pos2.X, pos2.Y, type2, 0, 2);

                //Kill all crystals
                foreach (var npc in Main.npc)
                    if (npc.active && npc.type != Type && npc.ModNPC is Godcat_CrystalNPC)
                        npc.StrikeInstantKill();
            }
            else
            {
                otherVehicle.defense -= 10;
            }

            // Screen shake
            var modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
            Main.instance.CameraModifiers.Add(modifier);
        }
        protected abstract void Move(Player player);
        protected abstract void BeginNextPhase(Player player);
        protected abstract void CreateHalfHealthHurtEffect();
        protected void SetAnimation(Asset<Texture2D> textureAsset, int fps)
        {
            if (textureAsset == null || !textureAsset.IsLoaded)
                return;

            currentTexture = textureAsset.Value;
            animationSpeed = fps / 60f;
            NPC.frameCounter = 0;
        }
        private void HandleStateChange()
        {
            StateTimer++;
            if (StateTimer >= stateDurations[currentState])
            {
                StateTimer = 0;
                //var index = Main.rand.Next(1, stateDurations.Count);
                if (currentState == State.Idle)
                {
                    var index = attackManager.Next();
                    currentState = stateDurations.ElementAt(index).Key;
                }
                else
                {
                    currentState = State.Idle;
                }

            }
        }
        private bool TryFindOtherVehicle(out NPC otherVehicle)
        {
            foreach (var npc in Main.npc)
            {
                if (npc.active && npc.type != NPC.type && npc.ModNPC is Godcat_VehicleNPC)
                {
                    otherVehicle = npc;
                    return true;
                }
            }

            otherVehicle = null;
            return false;
        }
        private void PunishFleeingPlayer(Player player)
        {
            if (currentState != State.Idle && NPC.Distance(player.Center) > PunishingDistance)
                FramesOverPunishDistance++;
            else
                FramesOverPunishDistance = 0;

            if (FramesOverPunishDistance > PunishFrameThreshold)
            {
                var position = NPC.Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 64;
                var velocity = NPC.DirectionTo(player.Center).RotatedByRandom(1f) * 20f;

                int type = 0;
                if (NPC.ModNPC is Godcat_Destroyer)
                    type = ModContent.ProjectileType<Godcat_DarkBlade>();

                else if (NPC.ModNPC is Godcat_Creator)
                    type = ModContent.ProjectileType<Godcat_LightBlade>();

                Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, type, NPC.damage, 3f);

                if (Main.GameUpdateCount % 5 == 0)
                    SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
            }
        }
    }
}
