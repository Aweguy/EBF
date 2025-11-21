using EBF.Abstract_Classes;
using EBF.Buffs;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class CrystalWing : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 30;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 70;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 8, platinum: 0);
            Item.rare = ItemRarityID.Cyan;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 1); // 30
                type = ModContent.ProjectileType<CrystalWingLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<CrystalWingSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddIngredient(ItemID.Ectoplasm, 15)
                .AddIngredient(ItemID.SoulofLight, 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class CrystalWingLauncher : EBFLauncher
    {
        private const float beamWidth = 0.5f;
        public override string Texture => "EBF/Items/Ranged/Guns/CrystalWing";
        public override void SetDefaults()
        {
            Projectile.width = 84;
            Projectile.height = 54;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item162;
            MaxCharge = 120;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Handle shooting sound (has to be done before the OnShoot() method is called)
            Player player = Main.player[Projectile.owner];
            if (player.HasBuff<Charged>())
            {
                ActiveTime = 90;
                ShootSound = SoundID.Item163;
            }
            else
            {
                ShootSound = SoundID.Item4;
            }
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            //Apply or remove buffs
            Player player = Main.player[Projectile.owner];
            if (player.HasBuff<Charged>())
            {
                player.ClearBuff(ModContent.BuffType<Charged>());
                var laserType = ModContent.ProjectileType<CrystalWingLaser>();
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, laserType, Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            else
            {
                player.AddBuff(ModContent.BuffType<Charged>(), 60 * 120);
            }
        }
    }
    public class CrystalWingSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 46;
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

    public class CrystalWingLaser : EBFDeathRay
    {
        private Entity Owner => Main.player[(int)Projectile.owner];
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Creator_HolyDeathray";
        protected override Vector3 LightColor => Color.White.ToVector3();
        public override void SetDefaultsSafe()
        {
            scaleFactor = 0.5f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
        }
        public override void AISafe()
        {
            Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);
            Projectile.Center = Owner.Center + Projectile.velocity * 48;
        }
    }
}
