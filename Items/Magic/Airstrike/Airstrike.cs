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

            Item.value = Item.sellPrice(copper: 0, silver: 5, gold: 10, platinum: 0);//Item's value when sold
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

    public class Airstrike_Bomb : ModProjectile
    {
        bool HasGoneDown = false;
        int GlowmaskOpacity = 255;

        bool ShakeLeft = true;
        bool ShakeRight = false;

        bool HasGottenBig = false;
        bool FromNPC = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.tileCollide = true;
            Projectile.hide = true;
            Projectile.extraUpdates = 2;
            DrawOffsetX = -13;
            DrawOriginOffsetY = -4;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            FromNPC = true;
            Explode();//Exploding after hitting an npc
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!HasGoneDown)
            {
                Projectile.position += Vector2.Normalize(oldVelocity) * 15f;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 60;

                HasGoneDown = true;
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

            if (HasGoneDown)
            {
                GlowmaskOpacity -= 255 / 100;

                if (Main.GameUpdateCount % 2 == 0)
                {
                    if (ShakeLeft)
                    {
                        Projectile.Center -= new Vector2(-2, 0);

                        ShakeLeft = false;
                        ShakeRight = true;

                    }
                    else if (ShakeRight)
                    {
                        Projectile.Center -= new Vector2(2, 0);

                        ShakeLeft = true;
                        ShakeRight = false;
                    }
                }
            }

            if (Projectile.timeLeft < 3)//Exploding after some time after hitting the ground
            {
                Explode();
            }

            return false;
        }

        private void Explode()
        {
            Projectile.tileCollide = false;

            Projectile.position = Projectile.Center;

            if (!HasGottenBig)
            {
                Projectile.width += 200;
                Projectile.height += 200;

                HasGottenBig = true;
            }

            Projectile.penetrate = -1;
            Projectile.Center = Projectile.position;

            if (FromNPC)
            {
                Projectile.Kill();
            }

        }

        public override void OnKill(int timeLeft)
        {
            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            // Smoke Dust spawn
            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
            }
            // Fire Dust spawn
            for (int i = 0; i < 80; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(255, 251, 0), 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 5;
                dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(255, 251, 0), 2f);
                Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 5;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 2; g++)
            {
                int goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("EBF/Items/Magic/Airstrike/Airstrike_Bomb_Glowmask").Value;

            if (HasGoneDown)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), new Color(255, 255, 255) * ((255 - GlowmaskOpacity) / 255f), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

            }
        }
    }

    public class Airstrike_SmallBomb : ModProjectile
    {
        bool HasGoneDown = false;
        int GlowmaskOpacity = 255;

        bool ShakeLeft = true;
        bool ShakeRight = false;

        bool HasGottenBig = false;
        bool FromNPC = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.tileCollide = true;
            Projectile.hide = true;

            Projectile.extraUpdates = 2;
            DrawOffsetX = -25;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            FromNPC = true;
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!HasGoneDown)
            {
                Projectile.position += Vector2.Normalize(oldVelocity) * 24f;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 60;

                HasGoneDown = true;
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
            if (HasGoneDown)
            {
                GlowmaskOpacity -= 255 / 100;

                if (Main.GameUpdateCount % 2 == 0)
                {
                    if (ShakeLeft)
                    {
                        Projectile.Center -= new Vector2(-2, 0);

                        ShakeLeft = false;
                        ShakeRight = true;

                    }
                    else if (ShakeRight)
                    {
                        Projectile.Center -= new Vector2(2, 0);

                        ShakeLeft = true;
                        ShakeRight = false;
                    }

                }
            }
            if (Projectile.timeLeft < 3)
            {
                Explode();
            }
            return false;
        }

        private void Explode()
        {
            Projectile.tileCollide = false;

            Projectile.position = Projectile.Center;

            if (!HasGottenBig)
            {
                Projectile.width += 100;
                Projectile.height += 100;

                HasGottenBig = true;
            }

            Projectile.penetrate = -1;
            Projectile.Center = Projectile.position;

            if (FromNPC)
            {
                Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            // Smoke Dust spawn
            for (int i = 0; i < 25; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 5;
            }
            // Fire Dust spawn
            for (int i = 0; i < 40; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(255, 251, 0), 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 3;
                dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(255, 251, 0), 2f);
                Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 3;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 2; g++)
            {
                int goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
            }
            Main.MouseWorld.ToScreenPosition();
            // reset size to normal width and height.
            Projectile.position.X = Projectile.position.X + Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y + Projectile.height / 2;
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.position.X = Projectile.position.X - Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y - Projectile.height / 2;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("EBF/Items/Magic/Airstrike/Airstrike_SmallBomb_Glowmask").Value;

            if (HasGoneDown)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), new Color(255, 255, 255) * ((255 - GlowmaskOpacity) / 255f), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }
        }


    }
}
