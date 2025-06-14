using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Materials
{
    public class NanoFibre : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.material = true;
            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 15, platinum: 0);
            Item.rare = ItemRarityID.Pink;
        }
    }
}
