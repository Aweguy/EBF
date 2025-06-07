using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using EBF.Abstract_Classes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using EBF.Items.Magic;

namespace EBF.NPCs.TownNPCs
{
    [AutoloadHead]
    public class Natalie : EBFTownNPC
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
            NPCID.Sets.HatOffsetY[NPC.type] = 4;
            NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<NatalieEmote>();

            NPC.Happiness
                .SetBiomeAffection<SnowBiome>(AffectionLevel.Like)
                .SetBiomeAffection<JungleBiome>(AffectionLevel.Dislike)
                .SetNPCAffection(ModContent.NPCType<Matt>(), AffectionLevel.Love)
                .SetNPCAffection(NPCID.Stylist, AffectionLevel.Like)
                .SetNPCAffection(NPCID.Nurse, AffectionLevel.Dislike)
                .SetNPCAffection(NPCID.ArmsDealer, AffectionLevel.Hate);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 30;
            NPC.height = 50;
            NPC.damage = 30;
            NPC.GivenName = "Natalie";
            AnimationType = NPCID.Guide;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.SurfaceMushroom,
                new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Natalie")
            ]);
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
        }
        public override void AddShops()
        {
            NPCShop shop = new(Type);
            shop.Add(ModContent.ItemType<NimbusRod>())
                .Add(ItemID.FruitSalad)
                .Add(ItemID.Marshmallow)
                .Add(ItemID.FancyDishes)
                .Add(ItemID.TheGuardiansGaze, Condition.MoonPhaseFirstQuarter)
                .Add(ItemID.TheTwinsHaveAwoken, Condition.MoonPhaseFull)
                .Add(ItemID.Dryadisque, Condition.MoonPhaseNew)
                .Add(ItemID.PoweredbyBirds, Condition.MoonPhases04)
                .Add(ItemID.StarryNight, Condition.MoonPhases15)
                .Add(ItemID.TheCreationoftheGuide, Condition.MoonPhases26)
                .Add(ItemID.TheEyeSeestheEnd, Condition.MoonPhases37)
                .Add(ItemID.TheScreamer, Condition.MoonPhasesEven)
                .Add(ItemID.Impact, Condition.MoonPhasesEvenQuarters)
                .Add(ItemID.UnicornCrossingtheHallows, Condition.MoonPhasesHalf0)
                .Add(ItemID.FacingtheCerebralMastermind, Condition.MoonPhasesHalf1)
                .Add(ItemID.SparkyPainting, Condition.MoonPhasesNearNew)
                .Add(ItemID.SomethingEvilisWatchingYou, Condition.MoonPhasesOdd)
                .Add(ItemID.GoblinsPlayingPoker, Condition.MoonPhasesOddQuarters)
                .Add(ItemID.TheDestroyer, Condition.MoonPhasesQuarter0)
                .Add(ItemID.GreatWave, Condition.MoonPhasesQuarter1)
                .Add(ItemID.TrioSuperHeroes, Condition.MoonPhasesQuarter2)
                .Add(ItemID.ThePersistencyofEyes, Condition.MoonPhasesQuarter3)
            .Register();
        }
        public override bool CanGoToStatue(bool toKingStatue) => !toKingStatue;
        public override bool CanTownNPCSpawn(int numTownNPCs) => NPC.downedBoss3;
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                shopName = "Shop";
            }
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 40;
            knockback = 4f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 10;
        }
        public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
        {
            Main.GetItemDrawFrame(ModContent.ItemType<ShootingStar>(), out item, out itemFrame);
            scale = 0.8f;
            horizontalHoldoutOffset = -8;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileID.FallingStar;
            attackDelay = 1;
        }
        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 7f;
            randomOffset = 0.1f;
        }
    }
}
