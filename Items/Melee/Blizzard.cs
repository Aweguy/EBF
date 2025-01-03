using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class Blizzard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 88;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 88;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 51;//Item's base damage value
            Item.knockBack = 4.5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 34;//How fast the item is used
            Item.useAnimation = 34;//How long the animation lasts. For swords it should stay the same as UseTime
            
            Item.value = Item.sellPrice(copper: 0, silver: 70, gold: 5, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.shoot = ProjectileID.FrostBlastFriendly;
            Item.shootSpeed = 4;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity + GetRandomVector() * 0.5f, type, damage, knockback);
            Projectile.NewProjectile(source, position, velocity + GetRandomVector() * 0.5f, type, damage, knockback);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Frostbrand, stack: 1)
                .AddIngredient(ItemID.FrostCore, stack: 1)
                .AddIngredient(ItemID.IceBlock, stack: 80)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        //It would be nice to move this method out of blizzard, but there's not enough vector logic to warrant an extension class.
        private Vector2 GetRandomVector() => new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
    }
}
