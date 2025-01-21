using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 8);
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
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ProjectileID.RocketI, Projectile.damage, Projectile.knockBack, Projectile.owner);
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

            ActiveTime = 30;
        }
        public override void WhileShoot(Vector2 barrelEnd, int type)
        {
            //Shoot twice
            if(Projectile.frameCounter == 0 || Projectile.frameCounter == 4)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
            }

            Projectile.frameCounter++;
        }
    }
}
