using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class DeepBlue : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 80;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 3);
                type = ModContent.ProjectileType<DeepBlueLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<DeepBlueSidearm>();
            }
        }
    }
    public class DeepBlueLauncher : EBFMagicLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/DeepBlue";
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 30;

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
            if (ProjectileExtensions.ClosestNPC(ref target, 400, Projectile.Center))
            {
                //Get ground below target
                Vector2 position = GetGroundPosition(target.Center);

                //Spawn projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<GeyserSpell>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
        private static Vector2 GetGroundPosition(Vector2 checkPosition)
        {
            Point pos = checkPosition.ToTileCoordinates();
            for (; pos.Y < Main.maxTilesY - 10 && Main.tile[pos.X, pos.Y] != null && !WorldGen.SolidTile2(pos.X, pos.Y); pos.Y++) { }

            return new Vector2(pos.X * 16 + 8, pos.Y * 16);
        }
    }
    public class DeepBlueSidearm : EBFSidearm
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

    public class GeyserSpell : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 2;

            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 10; i++)
            {
                //Spawn dirt dust
                Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(-2, 3)), DustID.Dirt, Vector2.Zero, 0, default, 5f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 30; i++)
            {
                //Spawn water dust
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, 0, DustID.GemSapphire, SpeedX: 0, SpeedY: Main.rand.Next(-8, -3), Scale: 3f);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void AI()
        {
            //Shoot up from the ground
            if (Projectile.height < 120)
            {
                Projectile.position.Y -= 5;
                Projectile.height += 5;
            }
        }
    }
}
