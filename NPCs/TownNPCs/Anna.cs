using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using EBF.Abstract_Classes;
using EBF.Items.Ranged.Guns;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using EBF.Items.Ranged.Bows;

namespace EBF.NPCs.TownNPCs
{
    public class Anna : EBFTownNPC
    {
        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[NPC.type] = 23;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[NPC.type] = 16 * 30;
            NPCID.Sets.AttackType[NPC.type] = 1;
            NPCID.Sets.AttackTime[NPC.type] = 30;
            NPCID.Sets.AttackAverageChance[NPC.type] = 30;
            NPCID.Sets.HatOffsetY[NPC.type] = 8;

            //The following line requires an emote bubble asset for Lance
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExamplePersonEmote>(); // Makes other NPCs talk about this NPC when in the world.

            NPC.Happiness
                .SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
                .SetBiomeAffection<UndergroundBiome>(AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.BestiaryGirl, AffectionLevel.Love)
                .SetNPCAffection(NPCID.Demolitionist, AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.Merchant, AffectionLevel.Hate);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 30;
            NPC.height = 50;
            NPC.damage = 30;
            NPC.GivenName = "Anna";
            AnimationType = NPCID.Nurse;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Anna")
            ]);
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
        }
        public override void AddShops()
        {
            NPCShop shop = new(Type);
            shop.Add(ModContent.ItemType<GaiasBow>())
                .Add(ItemID.Radar)
            .Register();
        }
        public override bool CanGoToStatue(bool toKingStatue) => !toKingStatue;
        public override bool CanTownNPCSpawn(int numTownNPCs) => NPC.downedQueenBee;
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
            var texture = TextureAssets.Item[ModContent.ItemType<FairyBow>()].Value;
            item = texture;
            itemFrame = texture.Bounds;
            horizontalHoldoutOffset = -4;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileID.WoodenArrowFriendly;
            attackDelay = 1;
        }
        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 8f;
            randomOffset = 0.1f;
        }
    }
}
