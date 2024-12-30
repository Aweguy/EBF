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
    public class BalanceBadge  : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Balance Badge");
            base.Tooltip.WithFormatArgs("Represents pure equilibrium and bestows a wealth of boosts.\n5 defense\nIncreases maximum health and mana by 10\nIncreases movement, attack speed, damage and crit chance by 5%\nIncreases max minion slots by 1");
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 19;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.defense = 5;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.05f;
            player.moveSpeed += 0.05f;
            player.statLifeMax2 += 10;
            player.statManaMax2 += 10;
            player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
            player.GetCritChance(DamageClass.Generic) += 5;
            player.maxMinions += 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<SwordMedal>(stack: 1)
                .AddIngredient<ShieldMedal>(stack: 1)
                .AddIngredient<AngryFaic>(stack: 1)
                .AddIngredient<GreenCross>(stack: 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
