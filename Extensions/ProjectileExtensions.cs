using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace EBF.Extensions
{
    public static class ProjectileExtensions
    {
        /// <summary>
        /// Adjusts a rotation towards a target angle in small increments, choosing the shortest direction. 
        /// <para>Mostly used to make things turn towards something while it moves forward.</para>
        /// </summary>
        /// <param name="currentRotation">The current rotation in radians.</param>
        /// <param name="targetAngle">The target rotation that the projectile should rotate towards.</param>
        /// <param name="speed">How much the projectile can rotate each time the method is called.</param>
        /// <returns>A rotation which is closer to the target angle than the current.</returns>
        public static float SlowRotation(float currentRotation, float targetAngle, float speed)//Taken from qwerty's mod
        {
            int f = 1; //this is used to switch rotation direction
            float actDirection = new Vector2(MathF.Cos(currentRotation), MathF.Sin(currentRotation)).ToRotation();
            targetAngle = new Vector2(MathF.Cos(targetAngle), MathF.Sin(targetAngle)).ToRotation();

            //this makes f 1 or -1 to rotate the shorter distance
            if (Math.Abs(actDirection - targetAngle) > Math.PI)
            {
                f = -1;
            }
            else
            {
                f = 1;
            }

            if (actDirection <= targetAngle + MathHelper.ToRadians(speed * 2) && actDirection >= targetAngle - MathHelper.ToRadians(speed * 2))
            {
                actDirection = targetAngle;
            }
            else if (actDirection <= targetAngle)
            {
                actDirection += MathHelper.ToRadians(speed) * f;
            }
            else if (actDirection >= targetAngle)
            {
                actDirection -= MathHelper.ToRadians(speed) * f;
            }
            actDirection = new Vector2(MathF.Cos(actDirection), MathF.Sin(actDirection)).ToRotation();

            return actDirection;
        }

        public delegate bool SpecialCondition(NPC possibleTarget);

        /// <summary>
        /// Finds the NPC that is closest to a given position.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="maxDistance">NPCs outside of this distance from the centerpoint will be ignored.</param>
        /// <param name="position">The centerpoint of the search.</param>
        /// <param name="ignoreTiles"></param>
        /// <param name="overrideTarget">If any target is provided, the method will always return that target if it's within the given range.</param>
        /// <param name="specialCondition">Allows special conditions, such as only targetting enemies not having local iFrames.</param>
        /// <returns>True if an npc has been found within the search range and meets all provided special conditions.</returns>
        public static bool ClosestNPC(ref NPC target, float maxDistance, Vector2 position, bool ignoreTiles = false, int overrideTarget = -1, SpecialCondition specialCondition = null)//Taken from qwerty's mod
        {
            //very advance users can use a delegate to insert special condition into the function like only targetting enemies not currently having local iFrames, but if a special condition isn't added then just return it true
            if (specialCondition == null)
            {
                specialCondition = delegate (NPC possibleTarget) { return true; };
            }
            bool foundTarget = false;
            //If you want to prioritse a certain target this is where it's processed, mostly used by minions that haave a target priority
            if (overrideTarget != -1)
            {
                if ((Main.npc[overrideTarget].Center - position).Length() < maxDistance && !Main.npc[overrideTarget].immortal && (Collision.CanHit(position, 0, 0, Main.npc[overrideTarget].Center, 0, 0) || ignoreTiles) && specialCondition(Main.npc[overrideTarget]))
                {
                    target = Main.npc[overrideTarget];
                    return true;
                }
            }
            //this is the meat of the targetting logic, it loops through every NPC to check if it is valid the minimum distance and target selected are updated so that the closest valid NPC is selected
            for (int k = 0; k < Main.npc.Length; k++)
            {
                NPC possibleTarget = Main.npc[k];
                float distance = (possibleTarget.Center - position).Length();
                if (distance < maxDistance && possibleTarget.active && possibleTarget.chaseable && !possibleTarget.dontTakeDamage && !possibleTarget.friendly && possibleTarget.lifeMax > 5 && !possibleTarget.immortal && (Collision.CanHit(position, 0, 0, possibleTarget.Center, 0, 0) || ignoreTiles) && specialCondition(possibleTarget))
                {
                    target = Main.npc[k];
                    foundTarget = true;

                    maxDistance = (target.Center - position).Length();
                }
            }
            return foundTarget;
        }

        /// <summary>
        /// Converts polar vectors into cartesian vectors.
        /// </summary>
        /// <param name="radius">The length of the vector.</param>
        /// <param name="theta">The angle of the vector.</param>
        /// <returns>A cartesian vector (A vector that has x and y coordinates).</returns>
        public static Vector2 PolarVector(float radius, float theta)
        {
            return new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
        }

        /// <summary>
        /// Generates a random vector.
        /// </summary>
        /// <returns>A vector where (X = -1 to 1, Y = -1 to 1).</returns>
        public static Vector2 GetRandomVector() => new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));

        /// <summary>
        /// Changes the hitbox rectangle of a given projectile.
        /// <br>Taken from Calamity Utilities.</br>
        /// </summary>
        /// <param name="projectile">The projectile whose hitbox will be expanded.</param>
        /// <param name="width">The new width of the projectile's hitbox.</param>
        /// <param name="height">The new height of the projectile's hitbox.</param>
        public static void ExpandHitboxBy(this Projectile projectile, int width, int height)
        {
            projectile.position = projectile.Center;
            projectile.width = width;
            projectile.height = height;
            projectile.position -= projectile.Size * 0.5f;
        }

        public static bool DrawProjectileCentered(this ModProjectile p, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[p.Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[p.Projectile.type], 0, p.Projectile.frame);
            Vector2 origin = frame.Size() / 2 + new Vector2(p.DrawOriginOffsetX, p.DrawOriginOffsetY);
            SpriteEffects effects = p.Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 drawPosition = p.Projectile.Center - Main.screenPosition + new Vector2(p.DrawOffsetX, 0);

            spriteBatch.Draw(texture, drawPosition, frame, lightColor, p.Projectile.rotation, origin, p.Projectile.scale, effects, 0f);

            return (false);
        }

        #region After Effects

        public static void DrawProjectileTrailCentered(this ModProjectile p, SpriteBatch spriteBatch, Color drawColor, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
        {
            Texture2D texture = TextureAssets.Projectile[p.Projectile.type].Value;

            p.DrawProjectileTrailCenteredWithTexture(texture, spriteBatch, drawColor, initialOpacity, opacityDegrade, stepSize);
        }

        public static void DrawProjectileTrailCenteredWithTexture(this ModProjectile p, Texture2D texture, SpriteBatch spriteBatch, Color drawColor, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
        {
            Rectangle frame = texture.Frame(1, Main.projFrames[p.Projectile.type], 0, p.Projectile.frame);
            Vector2 origin = frame.Size() / 2;
            SpriteEffects effects = p.Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[p.Projectile.type]; i += stepSize)
            {
                float opacity = initialOpacity - opacityDegrade * i;
                spriteBatch.Draw(texture, p.Projectile.oldPos[i] + p.Projectile.Hitbox.Size() / 2 - Main.screenPosition, frame, drawColor * opacity, p.Projectile.oldRot[i], origin, p.Projectile.scale, effects, 0f);
            }
        }

        #endregion After Effects
    }
}
