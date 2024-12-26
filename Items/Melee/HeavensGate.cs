using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using EBF.Extensions;

namespace EBF.Items.Melee
{
    public class HeavensGate : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Heaven's Gate");//Name of the Item
            base.Tooltip.WithFormatArgs("A legendary sword belonging to a line of famed corsairs.");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 30;//Item's base damage value
            Item.knockBack = 2f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<HeavensGate_LightBlade>();
            Item.shootSpeed = 5f;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextFloat() <= 0.3f)
            {
                int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.AncientLight);
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 VelocityManual = new Vector2(velocity.X, velocity.Y);//We store the velocity for later use

            Projectile.NewProjectile(source, Main.MouseWorld - (Vector2.Normalize(VelocityManual) * 80f), Vector2.Zero, type, damage, knockback, player.whoAmI, velocity.X, velocity.Y);//We use VelocityManual to push the created sword towards the player in reference of the mouse

            return false;
        }
    }

    public class HeavensGate_LightBlade : ModProjectile
    {
        float SpawnDistanceFromClick;
        bool DistanceSet = false;
        bool Stop = false;
        Vector2 SpawnPosition;
        Vector2 OldMouseWorld;
        int TrailSkip = 2;


        public override void SetStaticDefaults()//Mainly used for setting the frames of animations or things we don't want to change in the projectile
        {
            Main.projFrames[Projectile.type] = 11;
            ProjectileID.Sets.TrailingMode[Type] = 2; // Creates a trail behind the golf ball.
            ProjectileID.Sets.TrailCacheLength[Type] = 36; // Sets the length of the trail.
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.knockBack = 7f;
            Projectile.light = 1f;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.scale = 1.3f;
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 DustPosition = Projectile.position;
            Vector2 DustOldVelocity = Projectile.oldVelocity;
            DustOldVelocity.Normalize();
            DustPosition += DustOldVelocity * 16f;
            for (int i = 0; i < 20; i++)
            {
                int Light = Dust.NewDust(DustPosition, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 0, default(Color), 1f);
                Main.dust[Light].position = (Main.dust[Light].position + Projectile.Center) / 2f;
                Dust dust = Main.dust[Light];
                dust.velocity += Projectile.oldVelocity * 0.6f;
                dust = Main.dust[Light];
                dust.velocity *= 0.5f;
                Main.dust[Light].noGravity = true;
                DustPosition -= DustOldVelocity * 8f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float rotation = Main.rand.NextFloat(360);
            Vector2 Velocity = Projectile.velocity.RotatedBy(rotation * 0.0174533f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center - (Vector2.Normalize(Velocity) * 80f), Velocity, ModContent.ProjectileType<HeavensGate_LightBlade_Mini>(), hit.Damage, Projectile.knockBack, Projectile.owner, target.whoAmI, Projectile.whoAmI);
        }

        public override bool? CanDamage() //If it's not fully form, do not damage
        {
            if (Projectile.frame == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool PreAI()//Use this to write the AI of the projectile. Its behaviour in other words. Updates every frame.
        {
            if (!DistanceSet)//Setting the distance of the Projectile from the cursor
            {
                SpawnPosition = Main.MouseWorld - Vector2.Normalize(new Vector2(Projectile.ai[0], Projectile.ai[1])) * 80f;

                SpawnDistanceFromClick = Vector2.Distance(SpawnPosition, Main.MouseWorld);
                OldMouseWorld = Main.MouseWorld;
                DistanceSet = true;
            }

            Vector2 MoveSpeed = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            //Change the 5 to determine how much dust will spawn. lower for more, higher for less
            if (Main.rand.Next(3) == 0)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                Main.dust[dust].velocity.X *= 0.4f;
                Main.dust[dust].noGravity = true;
            }

            #region animation and more
            if (!Stop)
            {

                if (++Projectile.frameCounter > 2)
                {
                    Projectile.frameCounter = 0;

                    if (++Projectile.frame <= 3)
                    {
                        Projectile.velocity = Vector2.Zero;
                        Projectile.netUpdate = true;
                    }
                    else if (Projectile.frame == 4)
                    {
                        Projectile.velocity = Vector2.Normalize(MoveSpeed) * 16f;
                        Stop = true;
                    }

                    else if (Projectile.frame > 4)
                    {
                        Projectile.velocity = Vector2.Zero;

                        Projectile.netUpdate = true;

                        if (Projectile.frame == 11)
                        {
                            Projectile.Kill();
                        }
                    }
                }
            }

            if (Stop && Vector2.Distance(OldMouseWorld, Projectile.Center) >= SpawnDistanceFromClick * 2f)
            {
                Stop = false;
            }
            #endregion

            float velRotation = MoveSpeed.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;
            return false;
        }

        /*public override bool PreDraw(ref Color lightColor)
        {


            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            float initialOpacity = 0.8f;
            float opacityDegrade = 0.08f;



            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 1)
            {
                if(TrailSkip++ <= 0)
                {
                    float opacity = initialOpacity - opacityDegrade * i;
                    Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Hitbox.Size() / 2 - Main.screenPosition, frame, lightColor * opacity, Projectile.rotation, origin, Projectile.scale, effects, 0f);
                    TrailSkip = 2;
                }
            }

            #region Trailing

            #endregion Trailing
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin();

            return false;
        }*/



    }

    public class HeavensGate_LightBlade_Mini : ModProjectile
    {
        float SpawnDistanceFromTarget;
        bool DistanceSet = false;
        bool Stop = false;
        Vector2 SpawnPosition;
        Vector2 OldTargetPosition;
        Vector2 MoveSpeed;

        Projectile Father;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.light = 1f;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.scale = 1.3f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Father.localAI[0] <= 3f)
            {

                Projectile.localAI[0] = Father.localAI[0];
                Projectile.localAI[0]++;
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] <= 1)
                {
                    float rotation = Main.rand.NextFloat(360);
                    Vector2 Velocity = Projectile.velocity.RotatedBy(rotation * 0.0174533f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center - (Vector2.Normalize(Velocity) * 80f), Velocity, ModContent.ProjectileType<HeavensGate_LightBlade_Mini>(), hit.Damage, Projectile.knockBack, Projectile.owner, target.whoAmI, Projectile.whoAmI);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 DustPosition = Projectile.position;
            Vector2 DustOldVelocity = Projectile.oldVelocity;
            DustOldVelocity.Normalize();
            DustPosition += DustOldVelocity * 16f;
            for (int i = 0; i < 20; i++)
            {
                int Light = Dust.NewDust(DustPosition, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 0, default(Color), 1f);
                Main.dust[Light].position = (Main.dust[Light].position + Projectile.Center) / 2f;
                Dust dust = Main.dust[Light];
                dust.velocity += Projectile.oldVelocity * 0.6f;
                dust = Main.dust[Light];
                dust.velocity *= 0.5f;
                Main.dust[Light].noGravity = true;
                DustPosition -= DustOldVelocity * 8f;
            }
        }

        public override bool? CanDamage() //If it's not fully form, do not damage
        {
            if (Projectile.frame == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool PreAI()
        {
            NPC target = Main.npc[(int)Projectile.ai[0]];
            Father = Main.projectile[(int)Projectile.ai[1]];
            if (!DistanceSet)//Setting the distance of the Projectile from the cursor
            {

                SpawnPosition = target.Center - Vector2.Normalize(Projectile.velocity) * 80f;

                SpawnDistanceFromTarget = Vector2.Distance(SpawnPosition, target.Center);
                OldTargetPosition = target.Center;
                DistanceSet = true;
                MoveSpeed = Projectile.velocity;
            }

            //Change the 5 to determine how much dust will spawn. lower for more, higher for less
            if (Main.rand.Next(3) == 0)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                Main.dust[dust].velocity.X *= 0.4f;
            }

            #region animation and more
            if (!Stop)
            {
                if (++Projectile.frameCounter > 2)
                {
                    Projectile.frameCounter = 0;

                    if (++Projectile.frame <= 3)
                    {
                        Projectile.velocity = Vector2.Zero;
                    }
                    else if (Projectile.frame == 4)
                    {
                        Projectile.velocity = MoveSpeed;
                        Stop = true;
                    }
                    else if (Projectile.frame > 4)
                    {
                        Projectile.velocity = Vector2.Zero;

                        if (Projectile.frame == 11)
                        {
                            Projectile.Kill();
                        }
                    }
                }

            }

            if (Stop && Vector2.Distance(OldTargetPosition, Projectile.Center) >= SpawnDistanceFromTarget * 2f)
            {
                Stop = false;
            }
            #endregion

            float velRotation = MoveSpeed.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;

            return false;
        }
    }
}
