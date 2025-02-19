using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class SteelShark : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 30;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 16;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 2, platinum: 0);
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 15);
                type = ModContent.ProjectileType<SteelSharkLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<SteelSharkSidearm>();
            }
        }
        //Obtained from opening iron crates at 10%
    }
    public class SteelSharkLauncher : EBFLauncher
    {
        private int bulletsToShoot;
        public override string Texture => "EBF/Items/Ranged/Guns/SteelShark";
        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 32;

            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ActiveTime = 90;
            ShootSound = SoundID.Item14;
        }
        public override bool PreAISafe()
        {
            //Add some offset cuz the sprite is small
            Projectile.position += ProjectileExtensions.PolarVector(Projectile.width / 2, Projectile.velocity.ToRotation());
            return false;
        }
        public override void WhileShoot(Vector2 barrelEnd, int type)
        {
            //Run once
            if (Projectile.localAI[1] == 0)
            {
                bulletsToShoot = 50;
                Projectile.localAI[1] = 1;
            }

            //Release bullets
            if (bulletsToShoot > 0)
            {
                //Reduce firerate over time
                int rate = 51 - bulletsToShoot;
                if (Main.GameUpdateCount % rate == 0)
                {
                    //Shoot
                    SoundEngine.PlaySound(SoundID.Item11, Projectile.position);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity.RotatedByRandom(0.1d), type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    bulletsToShoot--;
                }
            }
        }
    }
    public class SteelSharkSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 42;
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
