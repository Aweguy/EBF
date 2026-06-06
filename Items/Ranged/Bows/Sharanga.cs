using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class Sharanga : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<Sharanga_HoldoutProjectile>();
        public override void SetDefaults()
        {
            Item.width = 20;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 78;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Cyan;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 10f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<EagleEye>(stack: 1)
                .AddIngredient(ItemID.ShroomiteBar, stack: 8)
                .AddIngredient(ItemID.MeteoriteBar, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class Sharanga_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<Sharanga_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/Sharanga";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 70;
            MaximumDrawTime = 100;
            DamageScale = 3f;
            VelocityScale = 2f;
            base.SetDefaults();
        }
    }

    public class Sharanga_Arrow : ModProjectile
    {
        private int bounces = 2;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.extraUpdates = 1; //Don't forget that extra updates also increases perceived velocity
            Projectile.penetrate = -1;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void AI()
        {
            //Trail
            Lighting.AddLight(Projectile.Center, TorchID.Blue);
            for (var i = 0; i < 5; i++)
            {
                var dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Vector2.Zero, Scale: 1f);
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
