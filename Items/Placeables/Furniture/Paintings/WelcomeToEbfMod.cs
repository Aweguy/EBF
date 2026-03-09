using EBF.Tiles.Furniture.Paintings;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Placeables.Furniture.Paintings
{
    public class WelcomeToEbfMod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables.Furniture.Paintings";

        public override void SetDefaults()
        {
            Item.width = 96;
            Item.height = 64;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = Item.buyPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.White;
            Item.createTile = ModContent.TileType<WelcomeToEbfModTile>();
        }
    }
}
