using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class TargetBadge : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Target Badge");//Name of the Item
            base.Tooltip.WithFormatArgs("Somehow putting a target on yourself makes you a better shot, who knew?\n1 defense\nIncreases critical chance by 10% for all types of damage.\nIncreases enemy aggression.");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)

            Item.defense = 2;

            Item.value = Item.sellPrice(copper:0, silver:50, gold:1, platinum:0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.aggro += 100;
            player.GetCritChance(DamageClass.Ranged) += 5f;
        }
    }
}
