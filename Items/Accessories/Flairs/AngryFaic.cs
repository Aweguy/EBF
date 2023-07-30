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
    public class AngryFaic : Flair
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Angry Faic");
            base.Tooltip.WithFormatArgs("Wearing this makes you so angry, you want something to be BLAMMED!\n2 defense\nIncreases critical chance by 8% for all types of damage.\nIncreases damage by 8%.\nIncreases enemy aggression.");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
            Item.defense = 2;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            

            player.GetCritChance(DamageClass.Generic) += 8;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.aggro += 450;
        }
    }
}
