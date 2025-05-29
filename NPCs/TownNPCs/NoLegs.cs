using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using EBF.Abstract_Classes;
using EBF.Items.Summon;

namespace EBF.NPCs.TownNPCs
{
    public class NoLegs : EBFTownNPC
    {
        private readonly int[] walkSequence = [0, 1, 2, 3, 4, 5];
        private readonly int[] idleSequence = [7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17];

        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[NPC.type] = 18;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 0;
            NPCID.Sets.AttackFrameCount[NPC.type] = 0;
            NPCID.Sets.DangerDetectRange[NPC.type] = 16 * 5;
            NPCID.Sets.AttackType[NPC.type] = -1;
            NPCID.Sets.AttackTime[NPC.type] = -1;
            NPCID.Sets.AttackAverageChance[NPC.type] = 1;
            NPCID.Sets.HatOffsetY[NPC.type] = 8;

            //The following line requires an emote bubble asset for Matt
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExamplePersonEmote>(); // Makes other NPCs talk about this NPC when in the world.

            NPC.Happiness
                .SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
                .SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.Truffle, AffectionLevel.Hate);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 30;
            NPC.height = 50;
            NPC.damage = 30;
            NPC.GivenName = "NoLegs";
        }
        public override bool PreAI()
        {
            if(NPC.velocity.X > 0)
            {
                NPC.spriteDirection = NPC.direction = 1;
            }
            else if(NPC.velocity.X < 0)
            {
                NPC.spriteDirection = NPC.direction = -1;
            }

            return true;
        }
        public override void FindFrame(int frameHeight)
        {
            if (Main.GameUpdateCount % 8 != 0)
                return;

            NPC.frameCounter++;
            
            if(NPC.velocity.Y == 0 && NPC.velocity.X != 0)
            {
                if (NPC.frameCounter > walkSequence.Length - 1)
                    NPC.frameCounter = 0;

                NPC.frame.Y = frameHeight * walkSequence[(int)NPC.frameCounter];
            }
            else if(NPC.velocity.Y == 0)
            {
                if (NPC.frameCounter > idleSequence.Length - 1)
                    NPC.frameCounter = 0;

                NPC.frame.Y = frameHeight * idleSequence[(int)NPC.frameCounter];
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.NoLegs")
            ]);
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
        }
        public override void AddShops()
        {
            NPCShop shop = new(Type);
            shop.Add(ModContent.ItemType<SteelBuckler>())
            .Register();
        }
        public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;
        public override bool CanTownNPCSpawn(int numTownNPCs) => NPC.downedBoss1;
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                shopName = "Shop";
            }
        }
    }
}
