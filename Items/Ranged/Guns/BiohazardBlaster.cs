using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class BiohazardBlaster : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 30;

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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 10);
                type = ModContent.ProjectileType<BiohazardBlasterLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<BiohazardBlasterSidearm>();
            }
        }
    }
    public class BiohazardBlasterLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/BiohazardBlaster";
        public override void SetDefaults()
        {
            Projectile.width = 92;
            Projectile.height = 54;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item151;
            ShootSound = SoundID.Item66;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity / 4, ModContent.ProjectileType<BiohazardCloud>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
        }
    }
    public class BiohazardBlasterSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 42;
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

    public class BiohazardCloud : ModProjectile
    {
        private const int maxSize = 256;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.timeLeft = 60 * 15;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            if(Main.GameUpdateCount % 2 == 0)
            {
                //Expand hitbox until max size
                if (Projectile.width < maxSize)
                {
                    Projectile.ExpandHitboxBy(Projectile.width + 4, Projectile.height + 4);
                }

                //Slow down
                Projectile.velocity *= 0.99f;

                //Spawn clouds
                Vector2 position = Projectile.position + Main.rand.NextVector2Square(0, Projectile.width / 2);
                Gore gore = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), position, ProjectileExtensions.GetRandomVector(), Main.rand.Next(435, 438), 0.5f + ((float)Projectile.width * 2 / maxSize));
                gore.alpha = 128;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * 5);
            target.AddBuff(BuffID.Venom, 60 * 5);
        }
    }
}
