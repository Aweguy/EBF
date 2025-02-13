using EBF.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic.Airstrike
{
    public class Airstrike_Remote : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 24;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 120;//Item's base damage value
            Item.knockBack = 2f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.HoldUp;//The animation of the item when used
            Item.useTime = 40;//How fast the item is used
            Item.useAnimation = 40;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 75, gold: 8, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item8;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.noMelee = true;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 60;
                Item.useAnimation = 60;
                Item.mana = 30;
                Item.shoot = ModContent.ProjectileType<Airstrike_SmallBomb>();
                Item.shootSpeed = 24f;
            }
            else
            {
                Item.useTime = 40;
                Item.useAnimation = 40;
                Item.mana = 10;
                Item.shoot = ModContent.ProjectileType<Airstrike_Bomb>();
                Item.shootSpeed = 16f;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int spawnAmount = 1;
            float spread = 0f;

            if (player.altFunctionUse == 2)
            {
                //Use different values for small bomb
                spread = 100f;
                spawnAmount = 3;
                damage /= 2;
            }

            for (int i = 0; i < spawnAmount; i++)
            {
                //Spawn position
                float offsetX = Main.rand.NextFloat(-401f, 401f);
                position = new Vector2(Main.MouseWorld.X + offsetX, Main.screenPosition.Y);
                position.Y -= 100 * i; //this is so they fall one by one

                //Velocity towards random point near cursor, with slight randomized speed
                Vector2 target = Main.MouseWorld + new Vector2(Main.rand.NextFloat(spread, -spread), 0);
                velocity = Vector2.Normalize(target - position) * velocity.Length();
                velocity *= Main.rand.NextFloat(0.9f, 1.1f);

                //Spawn the projectile
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            }

            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 12)
                .AddIngredient(ItemID.SoulofMight, stack: 8)
                .AddIngredient(ItemID.Bomb, stack: 30)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public abstract class EBFMissile : ModProjectile
    {
        protected Texture2D glowmaskTexture;
        protected float diggingDepth; //How far the missile is placed into the ground upon hitting it
        protected int explosionSize; //The hitbox size of the explosion
        protected int glowmaskOpacity = 0;

        private Vector2 clickPosition; //Used to let the projectile pass through tiles above the cursor
        private Vector2 shakeDirection = Vector2.UnitX * 3; //Increase the multiplier to make the shaking more intense
        private bool inGround = false;

        /// <summary>
        /// Sets the variables that are share identical values between all missiles types.
        /// <para>If one of these variables should differ between missiles, then move the variable into each subclass.</para>
        /// </summary>
        protected void SetEverythingElse()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.knockBack = 2f;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.extraUpdates = 1;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Face falling direction
            Projectile.rotation = Projectile.velocity.ToRotation();

            //Store click position for tile collision
            if (Main.myPlayer == Projectile.owner)
            {
                clickPosition = Main.MouseWorld;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.Kill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!inGround)
            {
                Projectile.position += Vector2.Normalize(oldVelocity) * diggingDepth;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 60;

                inGround = true;
            }

            return false;
        }
        public override bool PreAI()
        {
            //Tile collision
            HandleTileEnabling();

            if (inGround)
            {
                //Glow and shake
                glowmaskOpacity += 4;
                Shake();

                if (Projectile.timeLeft < 3)
                {
                    Projectile.Kill();
                }
            }

            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void PostDraw(Color lightColor)
        {
            if (inGround)
            {
                Main.spriteBatch.Draw(glowmaskTexture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, glowmaskTexture.Width, glowmaskTexture.Height), new Color(255, 255, 255) * (glowmaskOpacity / 255f), Projectile.rotation, glowmaskTexture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }
        }
        public override void OnKill(int timeLeft)
        {
            //Prevent this code from happening twice
            if (Projectile.localAI[0] == 1)
            {
                return;
            }

            Projectile.localAI[0] = 1;

            //Explode
            ProjectileExtensions.ExpandHitboxBy(Projectile, explosionSize, explosionSize);
            Projectile.Damage();


            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Dust dust;

            // Smoke Dust spawn
            for (int i = 0; i < 20; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 3f);
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 8;
            }
            // Fire Dust spawn
            for (int i = 0; i < 50; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Alpha: 100, newColor: Color.Yellow, Scale: Main.rand.NextFloat(1f, 4f));
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 3;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 4; g++)
            {
                Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, ProjectileExtensions.GetRandomVector() * 1.5f, Main.rand.Next(61, 64), Scale: 1.5f);
            }
        }
        private void HandleTileEnabling()
        {
            if (Projectile.position.Y >= clickPosition.Y)
            {
                Tile tile = Framing.GetTileSafely((int)(Projectile.position.X / 16), (int)(Projectile.position.Y / 16));

                if (tile == null || !tile.HasTile)
                {
                    Projectile.tileCollide = true;
                }
            }
        }
        private void Shake()
        {
            //Using frame counter instead of gameupdate because of extra updates
            Projectile.frameCounter++;
            if (Projectile.frameCounter <= 1)
            {
                Projectile.Center += shakeDirection;
                shakeDirection.X = -shakeDirection.X;
            }
            else
            {
                Projectile.frameCounter = 0;
            }
        }
    }

    public class Airstrike_Bomb : EBFMissile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            DrawOffsetX = -13;
            DrawOriginOffsetY = -4;

            explosionSize = 200; //The hitbox size of the explosion
            diggingDepth = 15; //How far the missile is placed into the ground upon hitting it

            glowmaskTexture = ModContent.Request<Texture2D>("EBF/Items/Magic/Airstrike/Airstrike_Bomb_Glowmask").Value;

            SetEverythingElse();
        }
    }

    public class Airstrike_SmallBomb : EBFMissile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            DrawOffsetX = -25;

            explosionSize = 100; //The hitbox size of the explosion
            diggingDepth = 24; //How far the missile is placed into the ground upon hitting it

            glowmaskTexture = ModContent.Request<Texture2D>("EBF/Items/Magic/Airstrike/Airstrike_SmallBomb_Glowmask").Value;

            SetEverythingElse();
        }
    }
}
