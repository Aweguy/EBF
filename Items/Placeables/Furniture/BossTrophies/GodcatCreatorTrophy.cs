﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using EBF.Tiles.Furniture.BossTrophies;

namespace EBF.Items.Placeables.Furniture.BossTrophies
{
    public class GodcatCreatorTrophy : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GodcatCreatorTrophyTile>());

            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 1);
        }
    }
}
