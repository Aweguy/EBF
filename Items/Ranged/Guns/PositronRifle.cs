using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class PositronRifle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 18;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 44;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 40, gold: 2, platinum: 0);
            Item.rare = ItemRarityID.LightRed;
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
                type = ModContent.ProjectileType<PositronRifleLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<PositronRifleSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.ClockworkAssaultRifle, stack: 1)
                .AddIngredient(ItemID.CrystalShard, stack: 15)
                .AddIngredient(ItemID.Amethyst, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class PositronRifleLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/PositronRifle";
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item113;
            ShootSound = SoundID.Item92;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            type = ModContent.ProjectileType<PositronRifle_PlasmaShot>();
            int explosionID = ModContent.ProjectileType<PositronRifle_PlasmaWave>();
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID);
        }
    }
    public class PositronRifleSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 18;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ShootSound = SoundID.Item114;
            ActiveTime = 30;
        }
        public override void WhileShoot(Vector2 barrelEnd, int type)
        {
            //Shoot twice
            if (Projectile.frameCounter == 0 || Projectile.frameCounter == 4)
            {
                int explosionID = 0;
                if (type == ProjectileID.Bullet)
                {
                    explosionID = ModContent.ProjectileType<PositronRifle_PlasmaBurst>();
                    type = ModContent.ProjectileType<PositronRifle_PlasmaShot>();
                }

                SoundEngine.PlaySound(ShootSound, Projectile.position);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID);
            }

            Projectile.frameCounter++;
        }
    }

    public class PositronRifle_PlasmaShot : ModProjectile
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
            Projectile.extraUpdates = 1; //because it's a bullet
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Face direction
            float velRotation = Projectile.velocity.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 4 == 0)
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
    public class PositronRifle_PlasmaBurst : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item132, Projectile.Center);
            Projectile.rotation = MathHelper.PiOver4 * Main.rand.Next(1, 5);
        }
        public override void AI()
        {
            //Run every other frame
            if (Main.GameUpdateCount % 4 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame > Main.projFrames[Projectile.type])
                {
                    Projectile.Kill();
                }
            }
        }
    }
    public class PositronRifle_PlasmaWave : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;

            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 40;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
        }
        public override void AI()
        {
            //Run every other frame
            if (Main.GameUpdateCount % 2 == 0)
            {
                int type = ModContent.ProjectileType<PositronRifle_PlasmaBurst>();
                Vector2 position = Projectile.position + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f)) * 128;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, type, Projectile.damage, 0, Projectile.owner);
            }
        }
    }
}
