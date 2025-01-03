using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class TargetBadge : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        //base.Tooltip.WithFormatArgs("Somehow putting a target on yourself makes you a better shot, who knew?\nIncreases critical chance by 4%.\nIncreases enemy aggression.");//Tooltip of the item

        public override void SetDefaults()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)
            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.aggro += 100;
            player.GetCritChance(DamageClass.Generic) += 4f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.SilverBar, stack: 10)
                .AddIngredient(ItemID.RedHusk, stack: 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
