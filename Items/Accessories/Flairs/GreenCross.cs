using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class GreenCross : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        private const int healthPercentage = 5;
        private const int healInterval = 60 * 10;
        public override void SetDefaults()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 28;//Height of the hitbox of the item (usually the item's sprite height)

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Lime;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (Main.GameUpdateCount % healInterval == 0 && player.statLife < player.statLifeMax2)
            {
                int healAmount = player.statLifeMax2 / 100 * healthPercentage;
                player.Heal(healAmount);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.ChlorophyteBar, stack: 20)
                .AddIngredient(ItemID.LifeFruit, stack: 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
