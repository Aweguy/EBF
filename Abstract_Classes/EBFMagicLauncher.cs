using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    /// <summary>
    /// This class represents a variation of EBFLauncher, that instead of shooting, will stab into the ground to create a magic effect.
    /// </summary>
    public abstract class EBFMagicLauncher : ModProjectile
    {
        /// <summary>
        /// The initial sound this item makes upon being used. Set this to an existing <see cref="SoundID"/> entry or assign to a new <see cref="SoundStyle"/> for a custom sound.
        /// <para/> Defaults to Item13 (aqua scepter sound).
        /// </summary>
        protected SoundStyle SpawnSound { get; set; } = SoundID.Item13;

        /// <summary>
        /// This hook is called once the weapon has been used.
        /// </summary>
        public virtual void OnSpawnSafe() { }

        /// <summary>
        /// Allows you to determine how this projectile behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns false by default.
        /// <br>Overriding this does not prevent the weapon from shooting or updating its position and rotation.</br>
        /// </summary>
        /// <returns>Whether or not to stop other AI.</returns>
        public virtual bool PreAISafe() { return false; }

        public override sealed bool ShouldUpdatePosition() => false;
        public override sealed void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SpawnSound, Projectile.position);
            Projectile.timeLeft = Main.player[Projectile.owner].itemTime;

            OnSpawnSafe();
        }
        public override sealed bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            HandleTransform(player);

            //Handle player arm rotation (this need to be improved because the arm can't go behind the player's head when Atan is used).
            Vector2 directionToGun = Vector2.Normalize(Projectile.position - player.Center);
            player.itemRotation = MathF.Atan2(directionToGun.Y * Projectile.direction, directionToGun.X * Projectile.direction);

            //Add collision when near ground
            if (Projectile.timeLeft < 2)
            {
                Projectile.tileCollide = true;
            }

            return PreAISafe();
        }
        private void HandleTransform(Player player)
        {
            //Set initial position to be inside player
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.position = playerCenter - Projectile.Size * 0.5f;

            //Update direction of sprite
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();

            //Very funny magic formula that gives the swing exponential speed, don't ask me how it works cuz idk.
            Projectile.frameCounter++;
            float itemTimePercent = (float)Projectile.frameCounter / (float)Projectile.timeLeft;
            float angle = MathHelper.Pi * (itemTimePercent / (player.itemTimeMax / 2));

            //Offset position using distance and angle
            Projectile.position -= ProjectileExtensions.PolarVector(Projectile.direction * 30, angle * Projectile.direction);

            //Handle rotation
            Projectile.rotation = (angle * Projectile.direction) - MathHelper.PiOver2 * Projectile.direction;
        }
    }
}
