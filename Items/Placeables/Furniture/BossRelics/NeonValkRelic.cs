using EBF.Tiles.Furniture.BossRelics;
using Terraria;
using Terraria.ModLoader;

namespace EBF.Items.Placeables.Furniture.BossRelics
{
    internal class NeonValkRelic : ModItem
    {
        public override void SetDefaults()
        {
            // The place style (here by default 0) is important if you decide to have more than one relic share the same tile type (more on that in the tiles' code)
            Item.DefaultToPlaceableTile(ModContent.TileType<NeonValkRelicTile>(), 0);

            Item.width = 30;
            Item.height = 40;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5);
        }
    }
}
