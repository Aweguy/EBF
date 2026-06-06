using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class TheStinger : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<TheStinger_HoldoutProjectile>();
        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 31;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 8;//How fast the item is used
            Item.useAnimation = 8;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 90, gold: 8, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<ThornBow>(stack: 1)
                .AddIngredient(ItemID.ChlorophyteBar, stack: 20)
                .AddIngredient(ItemID.Stinger, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class TheStinger_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ProjectileID.WoodenArrowFriendly;
        public override string Texture => "EBF/Items/Ranged/Bows/TheStinger";
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 66;
            MaximumDrawTime = 10;
            DamageScale = 1.0f;
            VelocityScale = 1.33f;
            base.SetDefaults();
        }

        protected override void OnShoot(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner,
            float ai0 = 0, float ai1 = 0, float ai2 = 0)
        {
            if (FullyCharged)
            {
                Projectile.Kill();
                for (var i = 0; i < 3; i++)
                {
                    var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.2d), ProjectileID.HornetStinger, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    proj.DamageType = DamageClass.Ranged;
                }
            }
            else
                base.OnShoot(source, position, velocity, type, damage, knockback, owner, ai0, ai1, ai2);
        }
    }
}
