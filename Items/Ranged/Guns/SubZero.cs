using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class SubZero : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 28;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 60;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 8, platinum: 0);
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 6);
                type = ModContent.ProjectileType<SubZeroLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<SubZeroSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<DeepBlue>(stack: 1)
                .AddIngredient(ItemID.IceBlock, stack: 50)
                .AddIngredient(ItemID.HallowedBar, stack: 10)
                .AddIngredient(ItemID.FrostCore, stack: 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class SubZeroLauncher : EBFMagicLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/SubZero";
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 56;

            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            SpawnSound = SoundID.Item1;
        }
        public override void OnGroundHit()
        {
            //Find a nearby target
            NPC target = new NPC();
            if (ProjectileExtensions.ClosestNPC(ref target, 500, Projectile.Center, ignoreTiles: true))
            {
                //Get ground below target
                Vector2 position = GetGroundPosition(target.Center);

                //Spawn projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<IcebergSpell>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
        private static Vector2 GetGroundPosition(Vector2 checkPosition)
        {
            Point pos = checkPosition.ToTileCoordinates();
            for (; pos.Y < Main.maxTilesY - 10 && Main.tile[pos.X, pos.Y] != null && !WorldGen.SolidTile2(pos.X, pos.Y); pos.Y++) { }

            return new Vector2(pos.X * 16 + 8, pos.Y * 16);
        }
    }
    public class SubZeroSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 28;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public class IcebergSpell : ModProjectile
    {
        public override string Texture => "EBF/Items/Ranged/Guns/SubZero_Iceberg";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 280;

            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.position.Y -= Projectile.height / 2;
            SoundStyle attackSound = SoundID.DeerclopsIceAttack with { Volume = 1.2f };
            SoundEngine.PlaySound(attackSound, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                //Spawn dirt dust
                Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(-2, 3) + Projectile.height), DustID.Dirt, Vector2.Zero, 0, default, 8f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                //Spawn ice dust
                Dust dust = Dust.NewDustDirect(Projectile.position + new Vector2(0, Projectile.height), Projectile.width, 0, DustID.Ice, SpeedX: 0, SpeedY: Main.rand.Next(-14, 0), Scale: 3f);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void AI()
        {
            if(Main.GameUpdateCount % 4 == 0)
            {
                //Handle animation
                Projectile.frame++;
                if (Projectile.frame > 3)
                {
                    Projectile.frame = 3;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, SpeedX: 0, SpeedY: 0, Scale: 2f); 
            }
        }
    }
}
