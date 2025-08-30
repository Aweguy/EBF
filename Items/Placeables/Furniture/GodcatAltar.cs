using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace EBF.Items.Placeables.Furniture
{
    public class GodcatAltar : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 50;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Furniture.GodcatAltarTile>();
            Item.rare = ItemRarityID.Red;
        }
    }
}
