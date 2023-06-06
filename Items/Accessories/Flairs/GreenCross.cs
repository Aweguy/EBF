using EBF.Abstract_Classes;
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
    public class GreenCross : Flair
    {
        int HealTimer = 60 * 5;//5 second timer

        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Green Cross");//Name of the Item
            base.Tooltip.WithFormatArgs("A Geneva-Convention-friendly pin which boasts regenerative properties.\\nRegenerates 5% of your maximum health every 10 seconds");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)

            Item.value = Item.sellPrice(copper:0, silver:50, gold:3, platinum:0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.lifeRegen += 5;
            player.lifeRegenCount += 5;
            player.lifeRegenTime -= 5;
            if(--HealTimer <= 0)
            {
                int regen = (player.statLifeMax2 / 100) * 5;
                player.Heal(regen);
                HealTimer = 60 * 5;
            }
        }
    }
}
