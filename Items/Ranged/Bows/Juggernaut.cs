using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class Juggernaut : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<Juggernaut_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 28;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 58;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 32;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 4, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Orange;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 10f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<IronTooth>(stack: 1)
                .AddIngredient(ItemID.HellstoneBar, stack: 20)
                .AddIngredient(ItemID.Grenade, stack: 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class Juggernaut_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<Juggernaut_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/Juggernaut";
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 58;
            MaximumDrawTime = 75;
            DamageScale = 2f;
            VelocityScale = 2f;
            ShootSound = SoundID.Item10;
            base.SetDefaults();
        }
    }

    public class Juggernaut_Arrow : ModProjectile
    {
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
        public override void AI()
        {
            Dust dust;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Scale: 2f);
            dust.noGravity = true;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 2f);
            dust.noGravity = true;
        }
        public override void OnKill(int timeLeft)
        {
            //Explode
            Projectile.Resize(64, 64);
            Projectile.CreateExplosionEffect(EBFUtils.ExplosionSize.Small);
            Projectile.Damage();

            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        }
    }
}
