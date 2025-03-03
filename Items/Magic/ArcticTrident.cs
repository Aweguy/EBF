
using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace EBF.Items.Magic
{
    public class ArcticTrident : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 60;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 9;//The amount of mana this item consumes on use

            Item.useTime = 42;//How fast the item is used
            Item.useAnimation = 42;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 80, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<ArcticTrident_Icecicle>();
            Item.shootSpeed = 12f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = StaffHead.DirectionTo(Main.MouseWorld).RotatedByRandom(0.1f) * Item.shootSpeed;

            //Spawn the projecile
            SoundEngine.PlaySound(SoundID.Item9, player.Center);
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<ArcticWind>(stack: 1)
                .AddIngredient(ItemID.IceBlock, stack: 40)
                .AddIngredient(ItemID.SoulofLight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class ArcticTrident_Icecicle : ModProjectile
    {
        private Vector2 initialVelocity; //Used to set velocity of smaller icecicles
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Save initial velocity
            initialVelocity = Projectile.velocity;

            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ice);
            }
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            //Slow over time
            Projectile.velocity *= 0.966f;

            //Countdown to death
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] == 30)
            {
                //Spawn smaller icecicles
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.position,
                        Vector2.Normalize(Projectile.velocity).RotatedByRandom(0.75f) * initialVelocity.Length(),
                        ModContent.ProjectileType<ArcticWind_Icecicle>(),
                        (int)(Projectile.damage * 0.5f),
                        Projectile.knockBack * 0.5f,
                        Projectile.owner
                        );
                }
                Projectile.Kill();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ice);
            }
        }
    }
}
