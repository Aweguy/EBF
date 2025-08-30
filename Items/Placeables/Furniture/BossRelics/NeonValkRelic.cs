using EBF.Tiles.Furniture.BossRelics;
using Terraria;
using Terraria.ModLoader;

namespace EBF.Items.Placeables.Furniture.BossRelics
{
    internal class NeonValkRelic : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<NeonValkRelicTile>());
            Item.width = 30;
            Item.height = 40;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5);
        }
    }
}
