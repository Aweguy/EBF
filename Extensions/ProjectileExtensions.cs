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
        /// <param name="speed">How much the projectile can rotate per step (In degrees).</param>
        /// <returns>A rotation which is closer to the target angle than the current.</returns>
        public static float SlowRotation(float currentRotation, float targetAngle, float speed)
        {
            float difference = MathHelper.WrapAngle(targetAngle - currentRotation); //reduces angle to pi / -pi
            float rotationStep = MathHelper.ToRadians(speed);

            if (Math.Abs(difference) <= rotationStep)
            {
                return targetAngle;
            }

            return currentRotation + Math.Sign(difference) * rotationStep;
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
            //Advanced users can use a delegate to insert special condition into the function, such as those without active iFrames
            //if a special condition isn't added then just return it true
            if (specialCondition == null)
            {
                specialCondition = delegate (NPC possibleTarget) { return true; };
            }
            bool foundTarget = false;
            if (overrideTarget != -1)
            {
                //Prioritizing a certain target happens here, mostly used by minions that have a target priority
                if ((Main.npc[overrideTarget].Center - position).Length() < maxDistance && !Main.npc[overrideTarget].immortal && (Collision.CanHit(position, 0, 0, Main.npc[overrideTarget].Center, 0, 0) || ignoreTiles) && specialCondition(Main.npc[overrideTarget]))
                {
                    target = Main.npc[overrideTarget];
                    return true;
                }
            }
            //Handles targeting logic, loops through each NPC to check if it is valid.
            //The minimum distance and target selected are updated so that only the closest valid NPC is selected.
            foreach(NPC npc in Main.npc)
            {
                float distance = (npc.Center - position).Length();
                if (distance < maxDistance && npc.active && npc.chaseable && !npc.dontTakeDamage && !npc.friendly && npc.lifeMax > 5 && !npc.immortal && (Collision.CanHit(position, 0, 0, npc.Center, 0, 0) || ignoreTiles) && specialCondition(npc))
                {
                    target = npc;
                    foundTarget = true;
                    maxDistance = distance;
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
        /// <br>From Calamity Utilities.</br>
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
    }
}
