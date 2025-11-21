using EBF.Items.Ranged.Guns;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items
{
    internal class EBFGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            switch (item.type)
            {
                #region Fishing Crates
                case ItemID.IronCrateHard:
                case ItemID.IronCrate:

                    //SteelShark at 10%
                    itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SteelShark>(), 10));
                    break;

                    #endregion
            }
        }
    }
}
