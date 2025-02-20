using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace EBF.Items.Accessories.Flairs
{
    public class BalanceBadge : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 19;
            Item.accessory = true;
            Item.value = Item.sellPrice(copper: 0, silver: 10, gold: 15, platinum: 0);//Item's value when sold
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
                .AddIngredient<AgnryFaic>(stack: 1)
                .AddIngredient<GreenCross>(stack: 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
