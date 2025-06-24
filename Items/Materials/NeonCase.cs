using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Materials
{
    public class NeonCase : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 28;
            Item.maxStack = 99;
            Item.material = true;
            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 15, platinum: 0);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
