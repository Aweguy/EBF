using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class ThornBow : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<ThornBow_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 62;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 16;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 26;//How fast the item is used
            Item.useAnimation = 26;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 55, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Orange;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<FairyBow>(stack: 1)
                .AddIngredient(ItemID.Stinger, stack: 10)
                .AddIngredient(ItemID.JungleSpores, stack: 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class ThornBow_HoldoutProjectile : EBFHoldoutBow
    {
        public override string Texture => "EBF/Items/Ranged/Bows/ThornBow";
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 62;
            MaximumDrawTime = 45;
            DamageScale = 1.0f;
            VelocityScale = 1.33f;
            base.SetDefaults();
        }

        protected override void OnShoot(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner,
            float ai0 = 0, float ai1 = 0, float ai2 = 0)
        {
            if (FullyCharged)
            {
                // Spawn thorns
                for (var i = 0; i < 3; i++)
                {
                    var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.2d), ProjectileID.HornetStinger, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    proj.DamageType = DamageClass.Ranged;
                }
            }
            
            base.OnShoot(source, position, velocity, type, damage, knockback, owner, ai0, ai1, ai2);
        }
    }
}
