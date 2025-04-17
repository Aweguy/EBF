using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using EBF.Abstract_Classes;
using EBF.Items.Ranged.Guns;

namespace EBF.NPCs.TownNPCs
{
    public class Lance : EBFTownNPC
    {
        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[NPC.type] = 25;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[NPC.type] = 16 * 5;
            NPCID.Sets.AttackType[NPC.type] = 3;
            NPCID.Sets.AttackTime[NPC.type] = 30;
            NPCID.Sets.AttackAverageChance[NPC.type] = 30;
            NPCID.Sets.HatOffsetY[NPC.type] = 8;

            //The following line requires an emote bubble asset for Matt
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExamplePersonEmote>(); // Makes other NPCs talk about this NPC when in the world.

            NPC.Happiness
                .SetBiomeAffection<MushroomBiome>(AffectionLevel.Like)
                .SetBiomeAffection<OceanBiome>(AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Love)
                .SetNPCAffection(NPCID.Cyborg, AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.Clothier, AffectionLevel.Hate);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 30;
            NPC.height = 50;
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
            .Register();
        }
        public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;
        public override bool CanTownNPCSpawn(int numTownNPCs) => Main.hardMode;
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                shopName = "Shop";
            }
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 70;
            knockback = 5f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 10;
        }
        public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight)
        {
            itemWidth = itemHeight = 64;
        }
        public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
        {
            Main.GetItemDrawFrame(ItemID.WoodenSword, out item, out itemFrame);
            itemSize = 16;
        }
    }
}
