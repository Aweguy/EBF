using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class IronTooth : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<IronTooth_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 18;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 46;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 19;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 40;//How fast the item is used
            Item.useAnimation = 40;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 10, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 7f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.PlatinumBar, stack: 10)
                .AddIngredient(ItemID.Grenade, stack: 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class IronTooth_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<IronTooth_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/IronTooth";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 46;
            MaximumDrawTime = 80;
            DamageScale = 1.75f;
            VelocityScale = 1.75f;
            ReleaseSound = SoundID.Item10;
            base.SetDefaults();
        }
    }
    public class IronTooth_Arrow : ModProjectile
    {
        private bool fullyCharged;
        public override string Texture => "EBF/Items/Ranged/Bows/Juggernaut_Arrow";
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

        public override void OnSpawn(IEntitySource source)
        {
            fullyCharged = (int)Projectile.ai[0] == 1;
        }

        public override void AI()
        {
            if (!fullyCharged)
                return;
            
            Dust dust;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Scale: 2f);
            dust.noGravity = true;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 2f);
            dust.noGravity = true;
        }
        public override void OnKill(int timeLeft)
        {
            if (!fullyCharged)
                return;
            
            //Explode
            Projectile.Resize(56, 56);
            Projectile.CreateExplosionEffect(EBFUtils.ExplosionSize.Small);
            Projectile.Damage();

            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        }
    }
}
