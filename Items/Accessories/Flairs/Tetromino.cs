
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class Tetromino : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)
            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var ModPlayer = player.GetModPlayer<EBFPlayer>();

            ModPlayer.tetrominoEquipped = true;
        }

        public override void AutoDefaults()
        {
            base.AutoDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.AdamantiteBar, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.TitaniumBar, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
