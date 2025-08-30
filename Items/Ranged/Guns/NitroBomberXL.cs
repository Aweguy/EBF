using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class NitroBomberXL : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 88;
            Item.height = 50;

            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 78;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 10, platinum: 0);
            Item.rare = ItemRarityID.Pink;
            Item.autoReuse = true;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                return player.HasAmmo(Item) && !player.HasBuff(ModContent.BuffType<Overheated>());
            }
            else
            {
                return player.HasAmmo(Item);
            }
        }
        public override bool AltFunctionUse(Player player) => true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 8);
                type = ModContent.ProjectileType<NitroBomberXLLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<NitroBomberXLSidearm>();
            }
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
