using System;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace EBF.EbfUtils
{
    public static partial class EBFUtils
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
        /// Basic homing method that adjusts the projectile's velocity slightly towards the target on each call.
        /// <br>Often combined with ProjectileExtensions.ClosestNPC() to home towards the nearest valid target.</br>
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="target">The npc this projectile should home towards.</param>
        /// <param name="strength">How significantly the velocity should be adjusted per step. Only accepts values above 0.</param>
        /// <param name="maxSpeed">The upper limit of how fast the projectile can become. Only accepts values between 0 and 100.</param>
        /// <returns>True if the projectile is currently homing towards a target, otherwise returns false.</returns>
        public static bool HomeTowards(this Projectile projectile, Entity target, float strength = 1, float maxSpeed = 100)
        {
            //Guard clauses
            if (target == null) return false;
            if (strength <= 0 || maxSpeed <= 0) return false;

            //Update velocity
            Vector2 towardsTarget = target.Center - projectile.Center;
            projectile.velocity += Vector2.Normalize(towardsTarget) * strength;

            //Limit speed if a valid speed is set.
            if(maxSpeed < 100 && projectile.velocity.Length() > maxSpeed)
            {
                projectile.velocity = Vector2.Normalize(projectile.velocity) * maxSpeed;
            }
            return true;
        }

        /// <summary>
        /// Rotates a projectile's sprite and velocity toward a target.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="target"></param>
        public static void LookAt(this Projectile projectile, Vector2 target)
        {
            //Initial rotation
            projectile.velocity = Vector2.Normalize(target - projectile.Center) * projectile.velocity.Length();
            projectile.rotation = projectile.velocity.ToRotation();

            //Account for directions
            int oldDirection = projectile.spriteDirection;
            if (oldDirection == -1)
                projectile.rotation += MathHelper.Pi;

            projectile.direction = projectile.spriteDirection = (projectile.velocity.X > 0).ToDirectionInt();

            if (projectile.spriteDirection != oldDirection)
                projectile.rotation -= MathHelper.Pi;
        }

        /// <summary>
        /// Enables the projectile's tile collision once it passes by the clicked position and is not currently touching a tile.
        /// <br>Often used by projectiles that appear from the sky.</br>
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="clickPosition">The clicked position, which is used to determine when the projectile should begin attempting to gain tile collision.</param>
        /// <param name="offset">The projectile will begin attempting to gain tile collision this many pixel above the clicked position. This makes the detection feel more consistent.</param>
        public static void HandleTileEnable(this Projectile projectile, Vector2 clickPosition, float offset = 32)
        {
            if (projectile.position.Y >= clickPosition.Y - offset)
            {
                Tile tile = Framing.GetTileSafely((int)(projectile.position.X / 16), (int)(projectile.position.Y / 16));
                if (tile == null || !tile.HasTile)
                {
                    projectile.tileCollide = true;
                }
            }
        }

        /// <summary>
        /// Sourced from Calamity Utils. Must only be used in projectile AI methods.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="healAmount">How much health the player should regain upon contacting this projectile.</param>
        /// <param name="playerToHeal">The index of the player that this projectile should heal.</param>
        /// <param name="homingVelocity">How quickly the projectile homes towards the player.</param>
        /// <param name="inertia">How slowly the trajectory of the projectile should change.</param>
        /// <param name="autoHomes"></param>
        /// <param name="timeCheck"></param>
        public static void ExecuteHealingProjectileAI(this Projectile projectile, int healAmount, int playerToHeal, float homingVelocity, float inertia, bool autoHomes = true, int timeCheck = 120)
        {
            Player player = Main.player[playerToHeal];
            Vector2 playerVector = player.Center - projectile.Center;
            float playerDist = playerVector.Length();

            //Check for overlaps
            if (playerDist < 50f && player.getRect().Intersects(projectile.getRect()))
            {
                if (projectile.owner == Main.myPlayer && !Main.player[Main.myPlayer].moonLeech)
                {
                    player.Heal(healAmount);
                    if (player.statLife > player.statLifeMax2)
                        player.statLife = player.statLifeMax2;

                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, playerToHeal, healAmount, 0f, 0f, 0, 0, 0);
                }
                projectile.Kill();
            }
            
            //Handle homing
            if (autoHomes || (player.lifeMagnet && projectile.timeLeft < timeCheck))
            {
                if (player.lifeMagnet)
                    homingVelocity *= 1.5f;

                playerDist = homingVelocity / playerDist;
                playerVector *= playerDist;
                projectile.velocity = (projectile.velocity * inertia + playerVector) / (inertia + 1f);
            }
        }
    }
}
