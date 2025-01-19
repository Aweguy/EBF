using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class SteelShark : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 32;

            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 20;
            Item.knockBack = 2;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.Green;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 5);
                type = ModContent.ProjectileType<SteelSharkLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<SteelSharkSidearm>();
            }
        }
    }
    public class SteelSharkLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/SteelShark";
        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 32;

            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item1;
            ShootSound = SoundID.Item14;
            MaxCharge = 10; //Gotta match usetime for this specific weapon because why should I have known that they wanted a launcher to be swung like a fucking sword???
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BrokenArmor, 60 * 10);
        }
        public override bool PreAISafe()
        {
            //WARNING: THIS AI IS VERY POORLY PROGRAMMED

            Player player = Main.player[Projectile.owner];
            HandleTransform(player);

            //Handle player arm rotation
            Vector2 directionToGun = Vector2.Normalize(Projectile.position - player.Center);
            player.itemRotation = MathF.Atan2(directionToGun.Y * Projectile.direction, directionToGun.X * Projectile.direction);

            return false;
        }
        public override void OnKill(int timeLeft)
        {
            //Explode
            Projectile.position += Vector2.UnitX * 40 * Projectile.direction; //Extend hitbox forward
            ProjectileExtensions.ExpandHitboxBy(Projectile, 100, 100);
            Projectile.friendly = true;
            Projectile.Damage();

            //Handle dust
            Dust dust;

            // Smoke
            for (int i = 0; i < 10; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 2f);
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 2;
            }
            // Fire
            for (int i = 0; i < 20; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Alpha: 100, newColor: Color.Yellow, Scale: Main.rand.NextFloat(1f, 2f));
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 1;
            }
        }
        private void HandleTransform(Player player)
        {
            //Calculate angle
            float itemTimePercent = (float)player.itemTime / (float)player.itemTimeMax;
            float angle = MathHelper.Pi * (1 - itemTimePercent);

            //Handle position
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.position = playerCenter - Projectile.Size * 0.5f;
            Projectile.position -= (Vector2.UnitX * 40 * Projectile.direction).RotatedBy(angle * Projectile.direction);

            //Handle rotation
            Projectile.rotation = (angle * Projectile.direction) + MathHelper.Pi;
        }
    }
    public class SteelSharkSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 22;

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
