using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class Destroyer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 24;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 32;
            Item.knockBack = 2;

            Item.value = Item.sellPrice(copper: 0, silver: 80, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.Orange;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 10);
                type = ModContent.ProjectileType<DestroyerLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<DestroyerSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.BeeWax, stack: 12)
                .AddIngredient(ItemID.JungleSpores, stack: 8)
                .AddIngredient(ItemID.VilePowder, stack: 20)
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
            Projectile.width = 36;
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
