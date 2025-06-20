﻿using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class ShadowBlaster : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 30;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 69;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 5, platinum: 0);
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 15);
                type = ModContent.ProjectileType<ShadowBlasterLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<ShadowBlasterSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddIngredient(ItemID.SoulofMight, 20)
                .AddIngredient(ItemID.Obsidian, 30)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class ShadowBlasterLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/ShadowBlaster";
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 52;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item117;
            ShootSound = SoundID.Item73;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            int explosionID = ModContent.ProjectileType<ShadowBlaster_AntimatterFissure>();
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ModContent.ProjectileType<ShadowBlaster_DarkShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID);
            proj.timeLeft = 30;
        }
    }
    public class ShadowBlasterSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 30;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            int explosionID = 0;
            if (type == ProjectileID.Bullet)
            {
                explosionID = ModContent.ProjectileType<ShadowBlaster_DarkExplosionSmall>(); //Sending hit effects through AI params so I can reuse the dark shot.
                type = ModContent.ProjectileType<ShadowBlaster_DarkShot>();
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID);
        }
    }

    public class ShadowBlaster_DarkShot : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 60 * 2;
            Projectile.extraUpdates = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Face direction
            float velRotation = Projectile.velocity.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;

            //Handle AntimatterFissure spawns
            if ((int)Projectile.ai[1] == 1)
            {
                Projectile.friendly = false;
            }

            //Use small explosion if no ID was provided
            if((int)Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = ModContent.ProjectileType<ShadowBlaster_DarkExplosionSmall>();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Prevent other shots from dealing damage.
            target.immune[Projectile.owner] = 10;
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 8 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            //Spawn the explosion that was passed down from the gun through the AI.
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, (int)Projectile.ai[0], Projectile.damage, 0, Projectile.owner);
        }
    }
    public class ShadowBlaster_DarkExplosionSmall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 2 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame == 7)
                {
                    Projectile.Kill();
                }
            }
        }
    }
    public class ShadowBlaster_AntimatterFissure : ModProjectile
    {
        private float rotation = 0;
        private int explosionID = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 13;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item82, Projectile.Center);
            explosionID = ModContent.ProjectileType<ShadowBlaster_DarkExplosionSmall>();
        }
        public override void AI()
        {
            //Wait 20 frames
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 20)
            {
                Projectile proj;

                //Spawn projectile above and below
                Vector2 velocity = new Vector2(3, 0).RotatedBy(rotation);
                proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<ShadowBlaster_DarkShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID, 1);
                proj.timeLeft = 30;

                Vector2 velocity2 = new Vector2(3, 0).RotatedBy(rotation + MathHelper.Pi);
                proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity2, ModContent.ProjectileType<ShadowBlaster_DarkShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID, 1);
                proj.timeLeft = 30;

                //Update rotation
                rotation += (float)Math.Tau / 16;
                Projectile.frameCounter = 16;
            }

            //Run once per 5 frames
            if (Main.GameUpdateCount % 8 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 12)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
