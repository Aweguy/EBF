using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using EBF.Items.Ranged.Guns;

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
