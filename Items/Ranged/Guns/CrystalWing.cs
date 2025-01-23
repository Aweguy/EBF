using EBF.Abstract_Classes;
using EBF.Buffs;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
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

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 70;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 2);
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
                .AddIngredient(ItemID.MarbleBlock, 150)
                .AddIngredient(ItemID.SoulofLight, 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class CrystalWingLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/CrystalWing";
        public override void SetDefaults()
        {
            Projectile.width = 84;
            Projectile.height = 54;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Player player = Main.player[Projectile.owner];
            if (player.HasBuff<Charged>())
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ProjectileID.FrostWave, Projectile.damage, Projectile.knockBack, Projectile.owner);
                proj.friendly = true;
                player.ClearBuff(ModContent.BuffType<Charged>());
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
}
