using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class CoconutShooter : EBFGun, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            base.SetDefaults();
            launcherType = ModContent.ProjectileType<CoconutShooterLauncher>();
            sidearmType = ModContent.ProjectileType<CoconutShooterSidearm>();
            overheatTime = 60 * 8;

            Item.width = 48;
            Item.height = 30;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.damage = 5;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 25, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.Blue;

            Item.shootSpeed = 8f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddRecipeGroup(RecipeGroupID.Wood, stack: 160)
                .AddIngredient(ItemID.Rope, stack: 50)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class CoconutShooterLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/CoconutShooter";
        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 52;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            ShootSound = SoundID.Item61;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            type = ModContent.ProjectileType<CoconutProjectile>();
            Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage * 2, Projectile.knockBack * 2, Projectile.owner);
            p.friendly = true;
        }
    }
    public class CoconutShooterSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 30;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            if (type == ProjectileID.Bullet)
            {
                type = ProjectileID.Seed;
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public class CoconutProjectile : ModProjectile
    {
        private int jumpHeight = 8; //Lets the coconut bounce less per hit
        public override string Texture => $"Terraria/Images/Item_{ItemID.Coconut}";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity = new Vector2(-Projectile.velocity.X * 0.25f, -jumpHeight);
            jumpHeight -= 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.aiStyle = ProjAIStyleID.Bounce;
            Projectile.penetrate = 5;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
        }
    }
}
