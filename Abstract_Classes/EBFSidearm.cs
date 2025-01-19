using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFSidearm : ModProjectile
    {
        /// <summary>
        /// The sound this item makes when shooting. Set this to an existing <see cref="SoundID"/> entry or assign to a new <see cref="SoundStyle"/> for a custom sound.
        /// <br/> For example <c>ShootSound = SoundID.Item11;</c> can be used for a bullet being fired.
        /// <para/> Defaults to Item11 (musket ball sound).
        /// </summary>
        protected SoundStyle ShootSound { get; set; } = SoundID.Item11;

        /// <summary>
        /// This hook is called when the weapon is supposed to shoot its projectile.
        /// </summary>
        public virtual void OnShoot(Vector2 barrelEnd, int type) { }

        /// <summary>
        /// Allows you to determine how this projectile behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns false by default.
        /// <br>Overriding this does not prevent the weapon from shooting or updating its position and rotation.</br>
        /// </summary>
        /// <returns>Whether or not to stop other AI.</returns>
        public virtual bool PreAISafe() { return false; }

        public override sealed bool ShouldUpdatePosition() => false;
        public override sealed bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            HandleTransform(player);

            //Handle player arm rotation
            player.itemRotation = MathF.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            //Run only once
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                SoundEngine.PlaySound(ShootSound, Projectile.position);

                //Identify bullet type
                if (player.PickAmmo(player.HeldItem, out int type, out _, out _, out _, out _, true))
                {
                    OnShoot(Projectile.Center + ProjectileExtensions.PolarVector(Projectile.width / 4, Projectile.velocity.ToRotation()), type);
                }
            }

            Projectile.timeLeft = player.itemTime + 1;

            return PreAISafe();
        }
        private void HandleTransform(Player player)
        {
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.position = playerCenter - Projectile.Size * 0.5f;

            Projectile.LookAt(Main.MouseWorld);
            Projectile.position += ProjectileExtensions.PolarVector(Projectile.width / 2, Projectile.velocity.ToRotation());
        }
    }
}
