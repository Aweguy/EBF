using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Materials
{
    public class RamChip : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 38;
            Item.maxStack = 9999;
            Item.material = true;
            Item.value = Item.buyPrice(copper: 0, silver: 80, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.Pink;
        }
    }
}
