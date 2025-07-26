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
    public abstract class Godcat : ModNPC
    {
        //Attacks
        protected enum State : byte { Idle, LightJudgmentWave, SeikenStorm, SeikenRing, ReturnBall, LightDiamondWalls }
        protected Dictionary<State, int> stateDurations;
        protected State currentState = State.Idle;
        protected ref float StateTimer => ref NPC.localAI[0];

        //Dodging
        private bool isDodging = false;
        private bool hasDodged = false; // Used to display dodging frames

        //Phases
        private const int PhaseDuration = 60 * 20; // How long the godcats stick around before summoning their vehicle
        private const int FinalPhaseDuration = 60 * 10; // How long the godcats stick around before finishing the fight
        protected ref float Phase => ref NPC.ai[0];
        private ref float PhaseTimer => ref NPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 46;
            NPC.damage = 100;
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
        public override void BossLoot(ref string name, ref int potionType)
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

            // Idle frames
            NPC.frameCounter += 0.1f;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type] - 2)
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
                return;
            }

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
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // The order in which you add loot will appear as such in the Bestiary. To mirror vanilla boss order:
            // 1. Trophy
            // 2. Classic Mode ("not expert")
            // 3. Expert Mode (usually just the treasure bag)
            // 4. Master Mode (relic first, pet last, everything else in between)

            // Trophy
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NeonValkTrophy>(), 10));

            // Classic Mode drops
            //npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<NanoFibre>(), 1, 3, 4));

            // Treasure bag
            //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NeonValkBossBag>()));

            // Relic
            //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.NeonValkRelic>()));

            // Pet
            //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<NeonValkPetItem>(), 4));
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
                var index = Main.rand.Next(1, stateDurations.Count);
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
            if (Phase < 2 && PhaseTimer > PhaseDuration)
            {
                SpawnDust();
                SummonVehicle(player);
                NPC.active = false;
            }
            else if (Phase == 2 && PhaseTimer > FinalPhaseDuration)
            {
                SpawnDust();
                NPC.StrikeInstantKill();
            }
        }
    }

    [AutoloadBossHead]
    public class Godcat_Light : Godcat
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/Godcat_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            stateDurations = new()
            {
                [State.Idle] = 200,
                //[State.SeikenStorm] = 120,
                //[State.SeikenRing] = 1,
                //[State.LightJudgmentWave] = 1,
                [State.LightDiamondWalls] = 200,
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Light")
            ]);
        }
        protected override void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(550, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.03f);
        }
        protected override void HandleAttacks(Player player)
        {
            //Don't attack in final phase
            if (Phase == 2)
            {
                return;
            }

            switch (currentState)
            {
                case State.Idle:
                    break;

                case State.SeikenStorm:
                    if (StateTimer % 4 == 0)
                        CreateStormSeiken(player);
                    break;

                case State.SeikenRing:
                    CreateSeikenRing(14, 10);
                    CreateSeikenRing(8, 6);
                    CreateSeikenRing(6, 4);
                    break;

                case State.LightJudgmentWave:
                    CreateJudgementWave(player);
                    break;

                case State.LightDiamondWalls:
                    if (StateTimer <= 120)
                    {
                        if (StateTimer % 40 == 0)
                        {
                            CreateDiamondWall(NPC.DirectionTo(player.Center) * 8f, 5, 50, 1.0f);
                        }
                        else if (StateTimer % 40 == 20)
                        {
                            CreateDiamondWall(NPC.DirectionTo(player.Center).RotatedBy(0.33f) * 8f, 5, 50, 1.0f);
                            CreateDiamondWall(NPC.DirectionTo(player.Center).RotatedBy(-0.33f) * 8f, 5, 50, 1.0f);
                        }
                    }
                    else if (StateTimer == 199)
                    {
                        CreateDiamondWall(NPC.DirectionTo(player.Center) * 4f, 7, 128, 2.0f);
                    }
                    break;
            }
        }
        protected override void SummonVehicle(Player player)
        {
            var pos = player.position.ToPoint() + new Point(-NPC.direction * 1600, 0);
            var type = ModContent.NPCType<Godcat_Creator>();
            NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type, 0, Phase);
        }
        protected override void SpawnDust()
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.AncientLight);
            }
        }
        private void CreateJudgementWave(Player player)
        {
            // Big ol' wave of judgement lasers
            CreateJudgementAt(player.Bottom);

            for (int i = 1; i < 5; i++)
            {
                var delay = 10 * i;
                var distance = 250 * i;
                CreateJudgementAt(player.Bottom + new Vector2(distance, 0), delay);
                CreateJudgementAt(player.Bottom + new Vector2(-distance, 0), delay);
            }
        }
        private void CreateJudgementAt(Vector2 position)
        {
            var type = ModContent.ProjectileType<Seraphim_Judgement>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position.ToGroundPosition().ToSurfacePosition(), Vector2.Zero, type, NPC.damage, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.friendly = false;
            proj.hostile = true;
        }
        private void CreateJudgementAt(Vector2 position, int delay)
        {
            var type = ModContent.ProjectileType<DelayedProjectile>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position.ToGroundPosition().ToSurfacePosition(), Vector2.Zero, type, NPC.damage, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.ai[0] = ModContent.ProjectileType<Seraphim_Judgement>();
            proj.timeLeft = delay;
            proj.friendly = false;
            proj.hostile = true;
        }
        private void CreateStormSeiken(Player player)
        {
            var position = NPC.Center + Main.rand.NextVector2CircularEdge(64, 64);
            var velocity = Vector2.Normalize(player.Center - position) * 14f;
            var type = ModContent.ProjectileType<Godcat_LightBlade>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity.RotatedByRandom(0.33f), type, NPC.damage, 3f, -1, NPC.target);

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_LightBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage, 3f, -1, NPC.target);
            }

            SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
        }
        private void CreateDiamondWall(Vector2 velocity, int amount, float spread, float scale)
        {
            SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound

            var type = ModContent.ProjectileType<Godcat_LightDiamond>();
            var rightAngleVector = new Vector2(-velocity.Y, velocity.X);
            rightAngleVector.Normalize();

            for (float i = -spread; i < spread; i += spread * 2 / amount)
            {
                var position = NPC.Center + rightAngleVector * i;
                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position, velocity, type, NPC.damage, 3f);
                proj.scale = scale;
            }
        }
    }
    [AutoloadBossHead]
    public class Godcat_Dark : Godcat
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/Godcat_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            stateDurations = new()
            {
                [State.Idle] = 200,
                [State.SeikenStorm] = 120,
                [State.SeikenRing] = 1,
                [State.ReturnBall] = 70,
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Dark")
            ]);
        }
        protected override void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(-550, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.05f);
        }
        protected override void HandleAttacks(Player player)
        {
            //Don't attack in final phase
            if (Phase == 2)
            {
                return;
            }

            switch (currentState)
            {
                case State.Idle:
                    break;

                case State.SeikenStorm:
                    if (StateTimer % 4 == 0)
                        CreateDarkStormSeiken(player);
                    break;

                case State.SeikenRing:
                    CreateDarkSeikenRing(14, 10);
                    CreateDarkSeikenRing(8, 6);
                    CreateDarkSeikenRing(6, 4);
                    break;

                case State.ReturnBall:
                    if (StateTimer % 30 == 0)
                    {
                        CreateDarkReturnBall(player, 15);

                        if (StateTimer == 0)
                            CreateDarkBallArc(player, 0.66f, 6, 9f);
                    }
                    break;
            }
        }
        protected override void SummonVehicle(Player player)
        {
            var pos = player.position.ToPoint() + new Point(-NPC.direction * 1600, 0);
            var type = ModContent.NPCType<Godcat_Destroyer>();
            NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type, 0, Phase);
        }
        protected override void SpawnDust()
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
            }
        }
        private void CreateDarkStormSeiken(Player player)
        {
            var position = NPC.Center + Main.rand.NextVector2CircularEdge(64, 64);
            var velocity = Vector2.Normalize(player.Center - position) * 14f;
            var type = ModContent.ProjectileType<Godcat_DarkBlade>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity.RotatedByRandom(0.33f), type, NPC.damage, 3f, -1, NPC.target);

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateDarkSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_DarkBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage, 3f, -1, NPC.target);
            }

            SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
        }
        private void CreateDarkReturnBall(Player player, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_ReturnBall>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * speed, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.DarkBig, NPC.whoAmI);

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateDarkBallArc(Player player, float spread, int amount, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_BallProjectile>();
            for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
            {
                var velocity = NPC.DirectionTo(player.Center).RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.DarkSmall);
            }
        }
    }
}
