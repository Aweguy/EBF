using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class AngryFaic : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 2, platinum: 0);//Item's value when sold
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
