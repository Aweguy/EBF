using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace EBF.Items.Magic
{
    public class NimbusRod : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 11;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 6;//The amount of mana this item consumes on use

            Item.useTime = 4;//How fast the item is used
            Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime
            Item.reuseDelay = 40;

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
            
            Item.shoot = ModContent.ProjectileType<NimbusRod_Bubble>();
            Item.shootSpeed = 15f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = StaffHead;
            velocity = position.DirectionTo(Main.MouseWorld).RotatedByRandom(0.1f) * Item.shootSpeed;
            velocity += Vector2.UnitY;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Coral, stack: 20)
                .AddIngredient(ItemID.Sapphire, stack: 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class NimbusRod_Bubble : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FlaironBubble}";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 60;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item85, Projectile.position);

            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Water);
            }
        }
        public override void AI()
        {
            //Slow over time
            Projectile.velocity *= 0.95f;

            //Go up
            Projectile.velocity -= Vector2.UnitY * 0.1f;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);

            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Water);
            }
        }
    }
}
