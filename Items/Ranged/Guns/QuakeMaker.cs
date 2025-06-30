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
    public class QuakeMaker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 28;

            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 21;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 60, gold: 0, platinum: 0);
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 6);
                type = ModContent.ProjectileType<QuakeMakerLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<QuakeMakerSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Amber, stack: 5)
                .AddIngredient(ItemID.GoldBar, stack: 10)
                .AddIngredient(ItemID.ScarabBomb, stack: 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class QuakeMakerLauncher : EBFMagicLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/QuakeMaker";
        public override void SetDefaults()
        {
            Projectile.width = 68;
            Projectile.height = 50;

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
            if (ProjectileExtensions.ClosestNPC(ref target, 400, Projectile.Center, ignoreTiles: true))
            {
                //Get ground below target
                Vector2 position = target.Center.ToGroundPosition();

                //Spawn projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<SandSpell>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
    }
    public class QuakeMakerSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 38;
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

    public class SandSpell : ModProjectile
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
            SoundEngine.PlaySound(SoundID.Item66, Projectile.Center);

            for (int i = 0; i < 10; i++)
            {
                //Spawn dirt dust
                Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(-2, 3)), DustID.Dirt, Vector2.Zero, 0, default, 5f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 30; i++)
            {
                //Spawn water dust
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, 0, DustID.GemTopaz, SpeedX: 0, SpeedY: Main.rand.Next(-8, -3), Scale: 3f);
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
