using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class FairyBow : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<FairyBow_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 20;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 8;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 35;//How fast the item is used
            Item.useAnimation = 35;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 6f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.RichMahogany, stack: 40)
                .AddIngredient(ItemID.BambooBlock, stack: 10)
                .AddIngredient(ItemID.Silk, stack: 8)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class FairyBow_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<FairyBow_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/FairyBow";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            MaximumDrawTime = 90;
            DamageScale = 1.0f;
            VelocityScale = 1.33f;
            ArrowDrawOffset = 8;
            base.SetDefaults();
        }

        protected override void OnShoot(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner,
            float ai0 = 0, float ai1 = 0, float ai2 = 0)
        {
            if (FullyCharged)
                for (int i = 0; i < 2; i++)
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
                        Projectile.velocity.RotatedByRandom(0.2d), ProjectileID.Leaf, (int)(Projectile.damage / 1.5f),
                        Projectile.knockBack, Projectile.owner);
            
            base.OnShoot(source, position, velocity, type, damage, knockback, owner, ai0, ai1, ai2);
        }
    }

    public class FairyBow_Arrow : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
    }
}
