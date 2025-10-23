using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using EBF.NPCs.Bosses;
using EBF.Items.Materials;
using EBF.NPCs.Bosses.NeonValkyrie;

namespace EBF.Items.TreasureBags
{
    public class NeonValkBossBag : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.TreasureBags";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.BossBag[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
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
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<NeonValkyrie>()));

            // Materials
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<NanoFibre>(), 1, 4, 5));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RamChip>(), 1, 8, 12));
            itemLoot.Add(ItemDropRule.Common(ItemID.ExplosivePowder, 1, 30, 50));
        }
    }
}
