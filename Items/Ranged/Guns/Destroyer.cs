using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class Destroyer : EBFGun, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            base.SetDefaults();
            sidearmType = ModContent.ProjectileType<DestroyerSidearm>();
            launcherType = ModContent.ProjectileType<DestroyerLauncher>();
            overheatTime = 60 * 12;

            Item.width = 40;
            Item.height = 24;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 32;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 80, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.Orange;

            Item.shootSpeed = 8f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.BeeWax, stack: 12)
                .AddIngredient(ItemID.JungleSpores, stack: 8)
                .AddRecipeGroup("EvilPowder", stack: 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class DestroyerLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/Destroyer";
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 32;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item151;
            ShootSound = SoundID.Item66;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            type = ModContent.ProjectileType<BiohazardCloud>();
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity / 4, type, Projectile.damage / 2, 0, Projectile.owner, 100);
        }
    }
    public class DestroyerSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 24;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}
