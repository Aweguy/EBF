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
    public class DeepBlue : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 32;

            Item.useTime = 30;
            Item.useAnimation = 30;
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
                type = ModContent.ProjectileType<DeepBlueLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<DeepBlueSidearm>();
            }
        }
    }
    public class DeepBlueLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/DeepBlue";
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
            MaxCharge = 30; //Gotta match usetime for this specific weapon because why should I have known that they wanted a launcher to be swung like a fucking sword???
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
        private void HandleTransform(Player player)
        {
            //Very funny magic formula that gives the swing exponential speed, don't ask me how it works cuz idk.
            Projectile.frameCounter++;
            float itemTimePercent = (float)Projectile.frameCounter / (float)Projectile.timeLeft;
            float angle = MathHelper.Pi * (itemTimePercent / (player.itemTimeMax / 3));

            //Handle position
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.position = playerCenter - Projectile.Size * 0.5f;
            Projectile.position -= (Vector2.UnitX * 30 * Projectile.direction).RotatedBy(angle * Projectile.direction);

            //Handle rotation
            Projectile.rotation = (angle * Projectile.direction) - MathHelper.PiOver2 * Projectile.direction;
        }
    }
    public class DeepBlueSidearm : EBFSidearm
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
