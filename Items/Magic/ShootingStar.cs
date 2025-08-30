using EBF.EbfUtils;
﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class ShootingStar : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        private const int spread = 250;
        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 36;//Item's base damage value
            Item.knockBack = 5;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 8;//The amount of mana this item consumes on use

            Item.useTime = 28;//How fast the item is used
            Item.useAnimation = 28;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item43;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
            
            Item.shoot = ModContent.ProjectileType<Star>();
            Item.shootSpeed = 5f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Spawn position
            float offsetX = Main.rand.NextFloat(-spread, spread);
            position = new Vector2(Main.MouseWorld.X + offsetX, Main.screenPosition.Y);

            //Velocity towards cursor
            velocity = Vector2.Normalize(Main.MouseWorld - position) * velocity.Length();

            //Spawn the projecile
            damage += Main.rand.Next(-20, 21);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.MeteoriteBar, stack: 15)
                .AddIngredient(ItemID.FallenStar, stack: 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class Star : ModProjectile
    {
        private const int dustsOnDeath = 50;
        private const int dustsOnBounce = 20;
        private const int maxVelocity = 16;
        private int bounces = 1;
        private bool isShrinking = false;
        private Vector2 clickPosition; //Used to let the projectile pass through tiles above the cursor
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                clickPosition = Main.MouseWorld;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.tileCollide)
            {
                Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
                if (bounces == 0)
                {
                    return true;
                }

                bounces--;
                isShrinking = true;

                //Bounce
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.5f;
                }

                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
                    Projectile.velocity.X *= 0.5f;
                }

                SpawnDusts(dustsOnBounce);
            }

            return false;
        }
        public override bool PreAI()
        {
            //Tile collision
            Projectile.HandleTileEnable(clickPosition);

            //Gravity & Terminal velocity
            Projectile.velocity.Y += 0.15f;
            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -maxVelocity, maxVelocity);

            //Trail
            if (Main.rand.NextBool(2))
            {
                SpawnDusts(1);
            }

            //Handle shrinking & despawning
            if (isShrinking)
            {
                Projectile.rotation -= 0.05f;
                Projectile.scale -= 0.01f;

                if (Projectile.scale <= 0)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.rotation += 0.33f;
            }

            return false;
        }
        public override void OnKill(int timeLeft)
        {
            SpawnDusts(dustsOnDeath);
        }
        private void SpawnDusts(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, SpeedX: 0, SpeedY: 0);
            }
        }
    }
}
