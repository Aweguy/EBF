using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class DragonsFeather : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 40;//Item's base damage value
            Item.knockBack = 15f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 12;//How fast the item is used
            Item.useAnimation = 12;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 10, silver: 20, gold: 4, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
        }
        public override void HoldItem(Player player)
        {
            player.AddBuff(BuffID.Swiftness, 1);
            player.AddBuff(BuffID.Sunflower, 1);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwiftBrand>(stack: 1)
                .AddIngredient(ItemID.SoulofFlight, stack: 20)
                .AddIngredient(ItemID.HallowedBar, stack: 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
