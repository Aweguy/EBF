using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class HeavyClaw : EBFGun, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            base.SetDefaults();
            launcherType = ModContent.ProjectileType<HeavyClawLauncher>();
            sidearmType = ModContent.ProjectileType<HeavyClawSidearm>();
            overheatTime = 60 * 8;

            Item.width = 48;
            Item.height = 30;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 72;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 5, platinum: 0);
            Item.rare = ItemRarityID.LightRed;

            Item.shootSpeed = 8f;
        }

        //Dropped by skeletron prime vice at 25% chance
        //Sold by Lance
    }
    public class HeavyClawLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/HeavyClaw";
        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 52;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ProjectileID.RocketI, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
    public class HeavyClawSidearm : EBFSidearm
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
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}
