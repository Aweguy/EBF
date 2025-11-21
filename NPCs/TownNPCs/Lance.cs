using EBF.Abstract_Classes;
using EBF.Items.Materials;
using EBF.Items.Ranged.Guns;
using EBF.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace EBF.NPCs.TownNPCs
{
    [AutoloadHead]
    public class Lance : EBFTownNPC
    {
        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[NPC.type] = 25;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[NPC.type] = 16 * 30;
            NPCID.Sets.AttackType[NPC.type] = 1;
            NPCID.Sets.AttackTime[NPC.type] = 30;
            NPCID.Sets.AttackAverageChance[NPC.type] = 30;
            NPCID.Sets.HatOffsetY[NPC.type] = 8;
            NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<LanceEmote>();

            NPC.Happiness
                .SetBiomeAffection<MushroomBiome>(AffectionLevel.Like)
                .SetBiomeAffection<OceanBiome>(AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Love)
                .SetNPCAffection(ModContent.NPCType<Anna>(), AffectionLevel.Like)
                .SetNPCAffection(NPCID.Cyborg, AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.Clothier, AffectionLevel.Hate);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 30;
            NPC.height = 48;
            NPC.damage = 30;
            NPC.GivenName = "Lance";
            AnimationType = NPCID.Guide;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.SurfaceMushroom,
                new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Lance")
            ]);
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
        }
        public override void AddShops()
        {
            NPCShop shop = new(Type);
            shop.Add(ModContent.ItemType<HeavyClaw>())
                .Add(ModContent.ItemType<ShadowBlaster>(), Condition.DownedDestroyer)
                .Add(ItemID.MusketBall)
                .Add(ItemID.ExplosivePowder)
                .Add(ItemID.UltrabrightTorch)
                .Add(ItemID.SpicyPepper)
                .Add(ItemID.Radar)
                .Add(ItemID.MetalDetector)
                .Add(ItemID.AmmoBox, Condition.BloodMoon)
                .Add(ModContent.ItemType<AtomicBattery>(), Condition.DownedSkeletronPrime)
                .Add(ModContent.ItemType<NeonCase>(), Condition.DownedTwins)
                .Add(ModContent.ItemType<MechanicalChain>(), Condition.DownedDestroyer)
                .Add(ModContent.ItemType<RamChip>(), new Condition("Mods.EBF.DownedNeonValk", () => DownedBossSystem.downedNeonValk))
                .Add(ModContent.ItemType<NanoFibre>(), new Condition("Mods.EBF.DownedNeonValk", () => DownedBossSystem.downedNeonValk))
            .Register();
        }
        public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;
        public override bool CanTownNPCSpawn(int numTownNPCs) => Main.hardMode;
        public override WeightedRandom<string> GetChatSafe(WeightedRandom<string> dialogue)
        {
            if (DownedBossSystem.downedNeonValk)
            {
                // Check if we've heard the line before
                var modPlayer = Main.LocalPlayer.GetModPlayer<EBFPlayer>();
                if (!modPlayer.hasHeardDownedNeonValkLine)
                {
                    modPlayer.hasHeardDownedNeonValkLine = true;
                    dialogue.Add(this.GetLocalizedValue("Chat.DownedNeonValk"), weight: 9999);
                }
            }
            return dialogue;
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                shopName = "Shop";
            }
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 60;
            knockback = 4f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 10;
        }
        public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
        {
            var texture = TextureAssets.Projectile[ModContent.ProjectileType<ShadowBlasterSidearm>()].Value;
            item = texture;
            itemFrame = texture.Bounds;
            scale = 0.8f;
            horizontalHoldoutOffset = -4;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<ShadowBlaster_DarkShot>();
            attackDelay = 1;
        }
        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 8f;
            randomOffset = 0.1f;
        }
    }
}
