using EBF.EbfUtils;
using EBF.Items.Magic;
using EBF.Items.Materials;
using EBF.Items.TreasureBags;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    [AutoloadBossHead]
    public class Godcat_Light : GodcatNPC
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/Godcat_Light_Preview",
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
                [State.Idle] = Main.expertMode ? 180 : 220,
                [State.GoingTowardsGround] = 9999999,
                [State.InGround] = 9999999,
                [State.SeikenStorm] = 120,
                [State.SeikenRing] = 1,
                [State.LightJudgmentWave] = 1,
                [State.LightDiamondWalls] = 170,
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Light")
            ]);
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Dialogue
            string text = Phase switch
            {
                0 => "To be summoned by a child of humanity! Do you crave judgement so badly?",
                1 => "Your resilience is nearly impressive.",
                2 => "Your world and your creations are impressive. Keep it so.",
                _ => ""
            };

            Main.NewText(text, Color.LightCyan);

            if (Phase == 0)
                SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0 }, NPC.Center);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // The order in which you add loot will appear as such in the Bestiary. To mirror vanilla boss order:
            // 1. Trophy
            // 2. Classic Mode ("not expert")
            // 3. Expert Mode (usually just the treasure bag)
            // 4. Master Mode (relic first, pet last, everything else in between)

            // Trophy
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GodcatTrophy>(), 10));

            // Classic Mode drops
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<ElixirOfLife>(), 1, 1, 2));
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<HolyGrail>(), 1, 1, 2));

            // Treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<GodcatBossBag>()));
            
            // Relic
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeables.Furniture.BossRelics.GodcatRelic>()));

            // Pet
            //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<GodcatPetItem>(), 4));
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
                            CreateDiamondWall(NPC.DirectionTo(player.Center) * 10f, 6, 50, 1.0f);
                        }
                        else if (StateTimer % 40 == 20)
                        {
                            CreateDiamondWall(NPC.DirectionTo(player.Center).RotatedBy(0.33f) * 10f, 6, 50, 1.0f);
                            CreateDiamondWall(NPC.DirectionTo(player.Center).RotatedBy(-0.33f) * 10f, 6, 50, 1.0f);
                        }
                    }
                    else if (StateTimer == 169)
                    {
                        CreateDiamondWall(NPC.DirectionTo(player.Center) * 8f, 12, 180, 2.0f);
                    }
                    break;
            }
        }
        protected override void SummonVehicle(Player player)
        {
            var pos = player.position.ToPoint() + new Point(-NPC.direction * 1600, 0);
            var type = ModContent.NPCType<Godcat_Creator>();
            NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type, 0, Phase);

            type = ModContent.NPCType<BlueCrystal>();
            var amount = Phase == 0 ? 2 : 1;
            for (var i = 0; i < amount; i++)
                NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type);

            //Dialogue
            var text = Phase == 0 ? "I pass my infallible judgement upon you." : "This ends now.";
            Main.NewText(text, Color.LightCyan);
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
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position.ToGroundPosition().ToSurfacePosition(), Vector2.Zero, type, NPC.damage / 4, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.friendly = false;
            proj.hostile = true;
        }
        private void CreateJudgementAt(Vector2 position, int delay)
        {
            var type = ModContent.ProjectileType<DelayedProjectile>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position.ToGroundPosition().ToSurfacePosition(), Vector2.Zero, type, NPC.damage / 4, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
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
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity.RotatedByRandom(0.33f), type, NPC.damage / 4, 3f, -1, NPC.target);

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_LightBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage / 4, 3f, -1, NPC.target);
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
                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position, velocity, type, NPC.damage / 4, 3f);
                proj.scale = scale;
            }
        }
    }
}
