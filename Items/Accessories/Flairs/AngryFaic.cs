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
    public class AngryFaic : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Angry Faic");
            base.Tooltip.WithFormatArgs("Wearing this makes you so angry, you want something to be BLAMMED!\n2 defense\nIncreases critical chance by 8%.\nIncreases enemy aggression.");
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
            Item.defense = 2;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.aggro += 450;
            player.GetCritChance(DamageClass.Generic) += 8;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<TargetBadge>(stack: 1)
                .AddIngredient(ItemID.AdamantiteBar, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
