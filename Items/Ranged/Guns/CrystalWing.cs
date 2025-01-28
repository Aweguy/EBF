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
            Item.knockBack = 2;

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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 60);
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
                ActiveTime = 60;
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
            }
            else
            {
                player.AddBuff(ModContent.BuffType<Charged>(), 60 * 120);
            }
        }
        public override void WhileShoot(Vector2 barrelEnd, int type)
        {
            Player player = Main.player[Projectile.owner];
            if (!player.HasBuff<Charged>()) //We check false instead of true because WhileShoot() runs after OnShoot() has cleared the buff.
            {
                //Randomize 
                Vector2 verticalOffset = (Projectile.Center - barrelEnd).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloatDirection() * beamWidth;
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd + verticalOffset, Projectile.velocity, ProjectileID.LaserMachinegunLaser, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
                proj.friendly = true;
                proj.penetrate = -1;
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
}
