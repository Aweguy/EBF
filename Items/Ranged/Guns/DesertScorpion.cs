using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using EBF.EbfUtils;

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
            NPC target = new();
            if (EBFUtils.ClosestNPC(ref target, 500, Projectile.Center, ignoreTiles: true))
            {
                //Get ground below target
                Vector2 position = target.Center.ToGroundPosition();

                //Spawn projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<SandDuneSpell>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
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
        public override string Texture => "EBF/Items/Ranged/Guns/DesertScorpion_SandDune";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 256;

            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.position.Y -= Projectile.height / 2;
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                //Spawn dirt dust
                Vector2 position = Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(-2, 3) + Projectile.height);
                Dust dust = Dust.NewDustPerfect(position, DustID.Dirt, Vector2.Zero, 0, default, 6f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                //Spawn amber dust
                Dust dust = Dust.NewDustDirect(Projectile.position + new Vector2(0, Projectile.height), Projectile.width, 0, DustID.GemAmber, SpeedX: 0, SpeedY: Main.rand.Next(-14, 0), Scale: 3f);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void AI()
        {
            //Shoot up from the ground
            if (Main.GameUpdateCount % 6 == 0)
            {
                Projectile.frame++;
                if(Projectile.frame >= 10)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
