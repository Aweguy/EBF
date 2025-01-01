using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic.Airstrike
{
    public class Airstrike_Remote : ModItem
    {
        private float offsetX = 20f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Airstrike Remote");//Name of the Item
            base.Tooltip.WithFormatArgs("Bombs away!!!!\nLeft click to quickly drop bombs down, right click to drop 3 weaker bombs at once.");//Tooltip of the item
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 24;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 50;//Item's base damage value
            Item.knockBack = 2f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.HoldUp;//The animation of the item when used
            Item.useTime = 40;//How fast the item is used
            Item.useAnimation = 40;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
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
                Item.damage = 60;
                Item.mana = 30;
                Item.shoot = ModContent.ProjectileType<Airstrike_SmallBomb>();
                Item.shootSpeed = 16f;
            }
            else
            {
                Item.damage = 120;
                Item.useTime = 40;
                Item.useAnimation = 40;
                Item.mana = 10;
                Item.shoot = ModContent.ProjectileType<Airstrike_Bomb>();
                Item.shootSpeed = 10f;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                for (int i = 0; i <= 2; i++)
                {
                    Vector2 target = Main.screenPosition + new Vector2(Main.mouseX + Main.rand.NextFloat(-100f, 100f), Main.mouseY);
                    float ceilingLimit = target.Y;
                    if (ceilingLimit > player.Center.Y - 200f)
                    {
                        ceilingLimit = player.Center.Y - 200f;
                    }

                    position = Main.MouseWorld + new Vector2((-(float)Main.rand.Next(-401, 401) + offsetX) * player.direction, -600f);
                    position.Y -= 100 * i;
                    Vector2 heading = target - position;
                    if (heading.Y < 0f)
                    {
                        heading.Y *= -1f;
                    }
                    if (heading.Y < 20f)
                    {
                        heading.Y = 20f;
                    }

                    heading.Normalize();
                    heading *= new Vector2(velocity.X, velocity.Y).Length();
                    velocity.X = heading.X;
                    velocity.Y = heading.Y + Main.rand.Next(-40, 41) * 0.02f;
                    Projectile.NewProjectile(source, position, velocity, type, 30, knockback, player.whoAmI, 0f, ceilingLimit);

                }
            }
            else
            {
                Vector2 target = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
                float ceilingLimit = target.Y;
                if (ceilingLimit > player.Center.Y - 200f)
                {
                    ceilingLimit = player.Center.Y - 200f;
                }

                position = Main.MouseWorld + new Vector2((-(float)Main.rand.Next(-401, 401) + offsetX) * player.direction, -600f);
                position.Y -= 100;
                Vector2 heading = target - position;
                if (heading.Y < 0f)
                {
                    heading.Y *= -1f;
                }
                if (heading.Y < 20f)
                {
                    heading.Y = 20f;
                }

                heading.Normalize();
                heading *= new Vector2(velocity.X, velocity.Y).Length();
                velocity.X = heading.X;
                velocity.Y = heading.Y + Main.rand.Next(-40, 41) * 0.02f;
                Projectile.NewProjectile(source, position, velocity, type, 60, knockback, player.whoAmI, 0f, ceilingLimit);
            }

            return false;
        }
    }

    public abstract class Missile : ModProjectile
    {
        protected float diggingDepth; //How far the missile is placed into the ground upon hitting it
        protected int explosionSize; //The hitbox size of the explosion

        protected int glowmaskOpacity = 0;
        protected bool inGround = false;

        protected bool shakeLeft = true;
        protected bool shakeRight = false;

        protected bool hasGottenBig = false;
        protected bool fromNPC = false;

        /// <summary>
        /// Sets the variables that are share identical values between all missiles types.
        /// <para>If one of these variables should differ between missiles, then move the variable into each subclass.</para>
        /// </summary>
        protected void SetEverythingElse()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.tileCollide = true;
            Projectile.hide = true;
            Projectile.extraUpdates = 2;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            fromNPC = true;
            Explode();//Exploding after hitting an npc
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
            if (Projectile.timeLeft > 60)
            {
                float velRotation = Projectile.velocity.ToRotation();
                Projectile.rotation = velRotation;
            }

            if (inGround)
            {
                glowmaskOpacity += 2;

                if (Main.GameUpdateCount % 2 == 0)
                {
                    if (shakeLeft)
                    {
                        Projectile.Center -= new Vector2(-2, 0);

                        shakeLeft = false;
                        shakeRight = true;

                    }
                    else if (shakeRight)
                    {
                        Projectile.Center -= new Vector2(2, 0);

                        shakeLeft = true;
                        shakeRight = false;
                    }
                }
            }

            if (Projectile.timeLeft < 3)//Exploding after some time after hitting the ground
            {
                Explode();
            }

            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        protected void Explode()
        {
            Projectile.tileCollide = false;

            Projectile.position = Projectile.Center;

            if (!hasGottenBig)
            {
                Projectile.width += explosionSize;
                Projectile.height += explosionSize;

                hasGottenBig = true;
            }

            Projectile.penetrate = -1;
            Projectile.Center = Projectile.position;

            if (fromNPC)
            {
                Projectile.Kill();
            }

        }
    }

    public class Airstrike_Bomb : Missile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            DrawOffsetX = -13;
            DrawOriginOffsetY = -4;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            explosionSize = 200; //The hitbox size of the explosion
            diggingDepth = 15; //How far the missile is placed into the ground upon hitting it

            SetEverythingElse();
        }
        public override void OnKill(int timeLeft)
        {
            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Dust dust;

            // Smoke Dust spawn
            for (int i = 0; i < 50; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 2f);
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 10;
            }
            // Fire Dust spawn
            for (int i = 0; i < 160; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Alpha: 100, newColor: Color.Yellow, Scale: Main.rand.NextFloat(2f, 3f));
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 5;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 8; g++)
            {
                Vector2 position = Projectile.position + new Vector2(Projectile.width / 2 - 24f, Projectile.height / 2 - 24f);
                Vector2 velocity = new Vector2(Main.rand.NextBool(2) ? 1.5f : -1.5f, Main.rand.NextBool(2) ? 1.5f : -1.5f);
            }
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("EBF/Items/Magic/Airstrike/Airstrike_Bomb_Glowmask").Value;

            if (inGround)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), new Color(255, 255, 255) * (glowmaskOpacity / 255f), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }
        }
    }

    public class Airstrike_SmallBomb : Missile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            DrawOffsetX = -25;

            explosionSize = 100; //The hitbox size of the explosion
            diggingDepth = 24; //How far the missile is placed into the ground upon hitting it

            SetEverythingElse();
        }
        public override void OnKill(int timeLeft)
        {
            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Dust dust;

            // Smoke Dust spawn
            for (int i = 0; i < 25; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 2f);
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 5;
            }
            // Fire Dust spawn
            for (int i = 0; i < 80; i++)
            {
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Alpha: 100, newColor: Color.Yellow, Scale: Main.rand.NextFloat(2f, 3f));
                dust.velocity += Vector2.Normalize(dust.position - Projectile.Center) * 3;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 4; g++)
            {
                Vector2 position = Projectile.position + new Vector2(Projectile.width / 2 - 24f, Projectile.height / 2 - 24f);
                Vector2 velocity = new Vector2(Main.rand.NextBool(2) ? 1.5f : -1.5f, 1.5f);

                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), position, velocity, Main.rand.Next(61, 64), Scale: 1.5f);
            }
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("EBF/Items/Magic/Airstrike/Airstrike_SmallBomb_Glowmask").Value;

            if (inGround)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), new Color(255, 255, 255) * (glowmaskOpacity / 255f), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }
        }
    }
}
