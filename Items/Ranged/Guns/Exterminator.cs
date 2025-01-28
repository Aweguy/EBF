using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class Exterminator : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 26;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 54;
            Item.knockBack = 2;

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 2, platinum: 0);
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 12);
                type = ModContent.ProjectileType<ExterminatorLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<ExterminatorSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<Destroyer>(stack: 1)
                .AddIngredient(ItemID.Ichor, stack: 15)
                .AddIngredient(ItemID.MythrilBar, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class ExterminatorLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/Exterminator";
        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 36;

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
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity / 4, type, Projectile.damage / 2, 0, Projectile.owner, 150, extraDebuff);
        }
    }
    public class ExterminatorSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 26;

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
