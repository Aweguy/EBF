using EBF.Abstract_Classes;
using EBF.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class NitroBomberXL : EBFGun, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            base.SetDefaults();
            launcherType = ModContent.ProjectileType<NitroBomberXLLauncher>();
            sidearmType = ModContent.ProjectileType<NitroBomberXLSidearm>();
            overheatTime = 60 * 8;

            Item.width = 88;
            Item.height = 50;

            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.damage = 78;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 10, platinum: 0);
            Item.rare = ItemRarityID.Pink;

            Item.shootSpeed = 8f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NanoFibre>(), stack: 3)
                .AddIngredient(ModContent.ItemType<RamChip>(), stack: 20)
                .AddIngredient(ItemID.ExplosivePowder, stack: 200)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class NitroBomberXLLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/NitroBomberXL";
        public override void SetDefaults()
        {
            Projectile.width = 88;
            Projectile.height = 50;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ProjectileID.MiniNukeRocketI, Projectile.damage, Projectile.knockBack, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ProjectileID.ClusterRocketI, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
    public class NitroBomberXLSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 34;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ProjectileID.RocketI, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}
