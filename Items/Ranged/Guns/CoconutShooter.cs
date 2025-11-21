using EBF.Abstract_Classes;
using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Guns
{
    public class CoconutShooter : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Guns";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 30;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 5;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(copper: 0, silver: 25, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.Blue;
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
                type = ModContent.ProjectileType<CoconutShooterLauncher>();
            }
            else
            {
                type = ModContent.ProjectileType<CoconutShooterSidearm>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddRecipeGroup(RecipeGroupID.Wood, stack: 160)
                .AddIngredient(ItemID.Rope, stack: 50)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class CoconutShooterLauncher : EBFLauncher
    {
        public override string Texture => "EBF/Items/Ranged/Guns/CoconutShooter";
        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 52;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            ShootSound = SoundID.Item61;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            type = ModContent.ProjectileType<CoconutProjectile>();
            Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage * 2, Projectile.knockBack * 2, Projectile.owner);
            p.friendly = true;
        }
    }
    public class CoconutShooterSidearm : EBFSidearm
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 30;

            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void OnShoot(Vector2 barrelEnd, int type)
        {
            if (type == ProjectileID.Bullet)
            {
                type = ProjectileID.Seed;
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelEnd, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public class CoconutProjectile : ModProjectile
    {
        private int jumpHeight = 8; //Lets the coconut bounce less per hit
        public override string Texture => $"Terraria/Images/Item_{ItemID.Coconut}";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity = new Vector2(-Projectile.velocity.X * 0.25f, -jumpHeight);
            jumpHeight -= 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.aiStyle = ProjAIStyleID.Bounce;
            Projectile.penetrate = 5;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
        }
    }
}
