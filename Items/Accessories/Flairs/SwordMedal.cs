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
    public class SwordMedal : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Sword Medal");//Name of the Item
            base.Tooltip.WithFormatArgs("True might is the mark of discipline, honor and courage.\n Increases Ranged and Melee damage by 20%");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.accessory = true;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 2, platinum: 0);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.2f;
            player.GetDamage(DamageClass.Melee) += 0.2f;
        }
    }
}
