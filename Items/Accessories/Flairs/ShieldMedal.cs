﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class ShieldMedal : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Shield Medal");
            base.Tooltip.WithFormatArgs("Aid others where you can. Let all be helped and loved throughout the realm.\n20 defense");
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 19;
            Item.accessory = true;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 5, platinum: 0);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 20;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 15)
                .AddIngredient(ItemID.SoulofSight, stack: 5)
                .AddIngredient(ItemID.SoulofMight, stack: 5)
                .AddIngredient(ItemID.SoulofFright, stack: 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
