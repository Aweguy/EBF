using EBF.Extensions;
using EBF.Items.Magic;
using EBF.Systems;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses
{
    public abstract class Godcat : ModNPC
    {
        private bool isDodging = false;
        private bool hasDodged = false; // Used to display dodging frames
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

            if (Main.player[NPC.target].dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                return;
            }

            //Handle dodging
            isDodging = Main.GameUpdateCount % 60 > 10;
            if (isDodging)
            {
                DodgeOverlappingProjectile();
            }
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
        protected void DodgeOverlappingProjectile()
        {
            Rectangle npcBox = NPC.Hitbox;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.friendly && !proj.minion && npcBox.Intersects(proj.Hitbox))
                {
                    hasDodged = true;
                }
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
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Light")
            ]);
        }
        public override void AI()
        {
            base.AI();

            Player player = Main.player[NPC.target];
            Move(player);

            //Handle attacks
            if (Main.GameUpdateCount % 200 == 0)
            {
                switch (Main.rand.Next(4))
                {
                    case 0:
                        CreateJudgementWave(player);
                        break;
                    case 1:
                        //Flurry of light blade
                        NPC.ai[0] = 30;
                        break;
                    case 2:
                        CreateSeikenRing(14, 10);
                        CreateSeikenRing(8, 6);
                        CreateSeikenRing(6, 4);
                        break;
                    case 3:
                        //Cast multiple return balls
                        NPC.ai[1] = 3;
                        CreateBallArc(player, 0.66f, 6, 9f);
                        break;
                }
            }

            //Handle light blade flurry
            if (Main.GameUpdateCount % 4 == 0 && NPC.ai[0] > 0)
            {
                CreateStormSeiken(player);
                NPC.ai[0]--;
            }

            //Handle return balls
            if (Main.GameUpdateCount % 25 == 0 && NPC.ai[1] > 0)
            {
                CreateReturnBall(player, 15);
                NPC.ai[1]--;
            }
        }
        private void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(550, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.03f);
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
        }
        private void CreateSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_LightBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage, 3f, -1, NPC.target);
            }
        }
        private void CreateReturnBall(Player player, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_LightReturnBall>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * speed, type, NPC.damage, 3f, -1, NPC.whoAmI);
        }
        private void CreateBallArc(Player player, float spread, int amount, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_LightBall>();
            for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
            {
                var velocity = NPC.DirectionTo(player.Center).RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f);
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
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Dark")
            ]);
        }
        public override void AI()
        {
            base.AI();

            Player player = Main.player[NPC.target];
            Move(player);

            //Handle attacks
            if (Main.GameUpdateCount % 200 == 100)
            {
                switch (Main.rand.Next(3))
                {
                    case 0:
                        //Flurry of dark blade
                        NPC.ai[0] = 30;
                        break;
                    case 1:
                        CreateDarkSeikenRing(14, 10);
                        CreateDarkSeikenRing(8, 6);
                        CreateDarkSeikenRing(6, 4);
                        break;
                    case 2:
                        //Cast multiple return balls
                        NPC.ai[1] = 3;
                        CreateDarkBallArc(player, 0.66f, 6, 9f);
                        break;
                }
            }

            //Handle dark blade flurry
            if (Main.GameUpdateCount % 4 == 0 && NPC.ai[0] > 0)
            {
                CreateDarkStormSeiken(player);
                NPC.ai[0]--;
            }

            //Handle return balls
            if (Main.GameUpdateCount % 25 == 0 && NPC.ai[1] > 0)
            {
                CreateDarkReturnBall(player, 15);
                NPC.ai[1]--;
            }
        }
        private void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(-550, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.05f);
        }
        private void CreateDarkStormSeiken(Player player)
        {
            var position = NPC.Center + Main.rand.NextVector2CircularEdge(64, 64);
            var velocity = Vector2.Normalize(player.Center - position) * 14f;
            var type = ModContent.ProjectileType<Godcat_DarkBlade>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity.RotatedByRandom(0.33f), type, NPC.damage, 3f, -1, NPC.target);
        }
        private void CreateDarkSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_DarkBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage, 3f, -1, NPC.target);
            }
        }
        private void CreateDarkReturnBall(Player player, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_DarkReturnBall>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * speed, type, NPC.damage, 3f, -1, NPC.whoAmI);
        }
        private void CreateDarkBallArc(Player player, float spread, int amount, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_DarkBall>();
            for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
            {
                var velocity = NPC.DirectionTo(player.Center).RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f);
            }
        }
    }
}
