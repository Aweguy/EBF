using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class BiohazardBlaster : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 28;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 86;
            Item.knockBack = 2;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 5, platinum: 0);
            Item.rare = ItemRarityID.Lime;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 12);
                type = ModContent.ProjectileType<BiohazardBlasterLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<BiohazardBlasterSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<Exterminator>(stack: 1)
                .AddIngredient(ItemID.ToxicFlask, stack: 1)
                .AddIngredient(ItemID.VialofVenom, stack: 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
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
            type = ModContent.ProjectileType<BiohazardCloud>();
            int extraDebuff = BuffID.Ichor;
            int extraDebuff2 = BuffID.Venom;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity / 4, type, Projectile.damage / 2, 0, Projectile.owner, 200, extraDebuff, extraDebuff2);
        }
    }
    public class BiohazardBlasterSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 28;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            if (type == ProjectileID.Bullet)
            {
                type = ModContent.ProjectileType<GasBullet>();
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    /// <summary>
    /// This is a expanding cloud which lingers, damages, poisons and stinks foes.
    /// <br>ai[0] determines the maximum size the cloud grows to, with a minimum of 50.</br>
    /// <br>ai[1] and ai[2] should store additional debuffs which are inflicted on foes.</br>
    /// </summary>
    public class BiohazardCloud : ModProjectile
    {
        private int maxSize;
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
        public override void OnSpawn(IEntitySource source)
        {
            maxSize = (int)Projectile.ai[0];
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 2 == 0)
            {
                //Expand hitbox until max size
                if (Projectile.width < maxSize)
                {
                    Projectile.ExpandHitboxBy(Projectile.width + 2, Projectile.height + 2);
                }

                //Slow down
                Projectile.velocity *= 0.99f;

                //Spawn clouds
                for (int i = 0; i < 1 + Projectile.width / 50; i++)
                {
                    Vector2 position = Projectile.position + Main.rand.NextVector2Square(0, Projectile.width);
                    Gore gore = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), position, ProjectileExtensions.GetRandomVector(), Type: Main.rand.Next(435, 438), Scale: 0.5f + ((float)Projectile.width * 2 / maxSize));
                    gore.alpha = 128;
                    gore.rotation = MathHelper.PiOver2 * Main.rand.Next(1, 5);
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * 5);
            target.AddBuff(BuffID.Stinky, 60 * 5);
            
            //Add extra debuffs if specified
            if (Projectile.ai[1] != 0)
            {
                target.AddBuff((int)Projectile.ai[1], 60 * 5);
            }
            if (Projectile.ai[2] != 0)
            {
                target.AddBuff((int)Projectile.ai[2], 60 * 5);
            }
        }
    }

    /// <summary>
    /// This is a bullet that explodes into a damaging gas cloud upon hitting a foe.
    /// </summary>
    public class GasBullet : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CursedBullet}";
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;

            Projectile.timeLeft = 60 * 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Face direction
            float velRotation = Projectile.velocity.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Spawn gas cloud
            int type = Main.rand.Next(511, 514);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, Projectile.damage / 2, 0, Projectile.owner);
        }
    }
}
