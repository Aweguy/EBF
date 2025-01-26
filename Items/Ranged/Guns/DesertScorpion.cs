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
    public class DesertScorpion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 36;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 74;
            Item.knockBack = 2;

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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 3);
                type = ModContent.ProjectileType<DesertScorpionLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<DesertScorpionSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<QuakeMaker>(stack: 1)
                .AddIngredient(ItemID.Amber, stack: 10)
                .AddIngredient(ItemID.HallowedBar, stack: 10)
                .AddIngredient(ItemID.AncientBattleArmorMaterial, stack: 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class DesertScorpionLauncher : EBFMagicLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/DesertScorpion";
        public override void SetDefaults()
        {
            Projectile.width = 102;
            Projectile.height = 48;

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
            if (ProjectileExtensions.ClosestNPC(ref target, 500, Projectile.Center))
            {
                //Get ground below target
                Vector2 position = GetGroundPosition(target.Center);

                //Spawn projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<SandDuneSpell>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
        private static Vector2 GetGroundPosition(Vector2 checkPosition)
        {
            Point pos = checkPosition.ToTileCoordinates();
            for (; pos.Y < Main.maxTilesY - 10 && Main.tile[pos.X, pos.Y] != null && !WorldGen.SolidTile2(pos.X, pos.Y); pos.Y++) { }

            return new Vector2(pos.X * 16 + 8, pos.Y * 16);
        }
    }
    public class DesertScorpionSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 36;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public class SandDuneSpell : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 2;

            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 15; i++)
            {
                //Spawn dirt dust
                Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(-2, 3)), DustID.Dirt, Vector2.Zero, 0, default, 6f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 50; i++)
            {
                //Spawn water dust
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, 0, DustID.GemTopaz, SpeedX: 0, SpeedY: Main.rand.Next(-18, 0), Scale: 5f);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void AI()
        {
            //Shoot up from the ground
            if (Projectile.height < 250)
            {
                Projectile.position.Y -= 25;
                Projectile.height += 25;
            }
        }
    }
}
