using EBF.Abstract_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria;

namespace EBF.Items.Accessories.Flairs
{
    public class BalanceBadge  : Flair
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Balance Badge");
            base.Tooltip.WithFormatArgs("Represents pure equilibrium and bestows a wealth of boosts.\n5 defense\n5% increase to all damage types\nIncreases maximum health and mana by 10\nIncreases movement and attack speed by 5%\nIncreases max minion slots by 1\nIncreases critical rates by 5%");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 19;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 5;
            player.GetDamage(DamageClass.Generic) += 0.05f;
            player.moveSpeed += 0.05f;
            player.statLifeMax2 += 10;
            player.statManaMax2 += 10;
            player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
            player.GetCritChance(DamageClass.Generic) += 5;
            player.maxMinions += 1;
        }
    }
}
