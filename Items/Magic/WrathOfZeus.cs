using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace EBF.Items.Magic
{
    public class WrathOfZeus : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 54;//Item's base damage value
            Item.knockBack = 2;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 14;//The amount of mana this item consumes on use

            Item.useTime = 80;//How fast the item is used
            Item.useAnimation = 80;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 5, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.UseSound = SoundID.Item109;
            Item.shoot = ModContent.ProjectileType<WrathOfZeus_ThunderBall>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Spawn the projecile
            velocity = (Main.MouseWorld - StaffHead) * 0.055f;
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<Tribolt>(stack: 1)
                .AddIngredient(ItemID.MartianConduitPlating, stack: 80)
                .AddIngredient(ItemID.Nanites, stack: 50)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class WrathOfZeus_ThunderBall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 70;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            //Slow over time
            Projectile.velocity *= 0.95f;
            
            //Animate
            if(Main.GameUpdateCount % 6 == 0)
            {
                Projectile.frame++;
                if(Projectile.frame > 4)
                {
                    Projectile.frame = 0;
                }
            }

            //Timer to lightning
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] > 40)
            {
                Vector2 position = (Projectile.Center - Vector2.UnitY * 600) + Vector2.UnitX * Main.rand.Next(-100, 100);
                Vector2 velocity = position.DirectionTo(Projectile.position).RotatedByRandom(0.2f) * 12;
                
                Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), position, velocity, ProjectileID.MartianTurretBolt, Projectile.damage, Projectile.knockBack, Projectile.owner);
                p.friendly = true;
                p.hostile = false;
                p.tileCollide = false;
                p.timeLeft = 70;
                p.penetrate = 2;
                p.extraUpdates = 2;
                
                Dust.NewDust(position, 0, 0, DustID.Electric);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
            }
        }
    }
}
