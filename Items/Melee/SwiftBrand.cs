using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class SwiftBrand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        //'It's so easy to swing around!' - Matt

        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 12;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 18;//How fast the item is used
            Item.useAnimation = 18;//How long the animation lasts. For swords it should stay the same as UseTime
            
            Item.value = Item.sellPrice(copper: 40, silver: 5, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
        }

        /* TODO: Increase player movement speed while holding the weapon.
         * also, make the crafting recipe work with silver bars, cuz right now it explicitly wants tungsten bars.
         */

        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.TungstenBar, 12)
                .AddIngredient(ItemID.Feather, 8)
                .AddIngredient(ItemID.Emerald, 4)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
