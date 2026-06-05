using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class EagleEye : EBFBow, ILocalizedModType
    {
        protected override int HoldoutProjectile => ModContent.ProjectileType<EagleEye_HoldoutProjectile>();
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 36;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 37;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 10, gold: 8, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Cog, stack: 30)
                .AddIngredient(ItemID.TinBar, stack: 20)
                .AddIngredient(ItemID.SoulofSight, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class EagleEye_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<EagleEye_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/EagleEye";
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 66;
            MaximumDrawTime = 100;
            DamageScale = 2.5f;
            VelocityScale = 2f;
            base.SetDefaults();
        }
    }

    public class EagleEye_Arrow : ModProjectile
    {
        private int bounces = 1;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.extraUpdates = 1; //Don't forget that extra updates also increases perceived velocity
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.hide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void AI()
        {
            //Trail
            Lighting.AddLight(Projectile.Center, TorchID.White);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Vector2.Zero, Scale: 1f);
                dust.position -= Projectile.velocity / 5f * i;
                dust.noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            //Bounce
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.8f;

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.8f;

            Projectile.ResetLocalNPCHitImmunity();

            return bounces-- <= 0;
        }
    }
}
