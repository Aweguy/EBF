using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class ThePhantom : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 22;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 182;
            Item.knockBack = 2;

            Item.value = Item.sellPrice(copper: 0, silver: 90, gold: 10, platinum: 0);
            Item.rare = ItemRarityID.Red;
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
                player.AddBuff(ModContent.BuffType<Overheated>(), 60 * 15);
                type = ModContent.ProjectileType<ThePhantomLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<ThePhantomSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<ShadowBlaster>(stack: 1)
                .AddIngredient(ItemID.LunarBar, stack: 15)
                .AddIngredient(ItemID.Obsidian, stack: 80)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class ThePhantomLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/ThePhantom";
        public override void SetDefaults()
        {
            Projectile.width = 82;
            Projectile.height = 38;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;

            ChargeSound = SoundID.Item117;
            ShootSound = SoundID.Item73;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            int explosionID = ModContent.ProjectileType<ShadowBlaster_AntimatterFissure>();
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, ModContent.ProjectileType<ShadowBlaster_DarkShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID);
            proj.timeLeft = 45;
        }
    }
    public class ThePhantomSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 22;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            int explosionID = 0;
            if (type == ProjectileID.Bullet)
            {
                explosionID = ModContent.ProjectileType<ShadowBlaster_DarkExplosionSmall>(); //Sending hit effects through AI params so I can reuse the dark shot.
                type = ModContent.ProjectileType<ShadowBlaster_DarkShot>();
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner, explosionID);
        }
    }
}
