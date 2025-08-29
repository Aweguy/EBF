using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using EBF.Items.Materials;
using EBF.NPCs.Bosses.Godcat;

namespace EBF.Items.TreasureBags
{
    public class GodcatBossBag : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.TreasureBags";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.BossBag[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.expert = true;
        }
        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
        }
        public override bool CanRightClick() => true;
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Money
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<Godcat_Light>()));

            // Materials
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ElixirOfLife>(), 1, 2, 3));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HolyGrail>(), 1, 2, 3));
        }
    }
}
