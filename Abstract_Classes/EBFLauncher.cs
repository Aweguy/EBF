using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFLauncher : ModProjectile
    {
        private int charge;

        /// <summary>
        /// How many ticks it takes from charging the shot to shooting the shot.
        /// <br>Set this to 60 if you want the charge-up to take one second.</br>
        /// <para>Defaults to 30.</para>
        /// </summary>
        protected int MaxCharge { get; set; } = 30;
        
        /// <summary>
        /// The initial sound this item makes upon being used. Set this to an existing <see cref="SoundID"/> entry or assign to a new <see cref="SoundStyle"/> for a custom sound.
        /// <para/> Defaults to Item13 (aqua scepter sound).
        /// </summary>
        protected SoundStyle ChargeSound { get; set; } = SoundID.Item13;
        
        /// <summary>
        /// The sound this item makes when shooting. Set this to an existing <see cref="SoundID"/> entry or assign to a new <see cref="SoundStyle"/> for a custom sound.
        /// <br/> For example <c>ShootSound = SoundID.Item11;</c> can be used for a bullet being fired.
        /// <para/> Defaults to Item99 (dart rifle sound).
        /// </summary>
        protected SoundStyle ShootSound { get; set; } = SoundID.Item99;

        /// <summary>
        /// This hook is called when the weapon is supposed to shoot its projectile.
        /// </summary>
        /// <param name="barrelEnd">The approximate position of the launcher's barrel.</param>
        public virtual void OnShoot(Vector2 barrelEnd) { }

        /// <summary>
        /// This hook is called once the weapon has been used.
        /// </summary>
        public virtual void OnChargeBegin() { }

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
            HandleCharge(player);

            Projectile.timeLeft = player.itemTime + 1;

            //Handle player arm rotation
            player.itemRotation = MathF.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            return PreAISafe();
        }
        private void HandleTransform(Player player)
        {
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.position = playerCenter - Projectile.Size * 0.5f;

            Projectile.LookAt(Main.MouseWorld);
        }
        protected void HandleCharge(Player player)
        {
            if (charge == 0)
            {
                SoundEngine.PlaySound(ChargeSound, Projectile.position);
                OnChargeBegin();
            }

            if (charge < MaxCharge)
            {
                charge++;

                if (player.itemTime < 1)
                {
                    player.itemTime = 1;
                    player.itemAnimation = 1;
                    Projectile.timeLeft = 1;
                }
            }
            if (charge >= MaxCharge)
            {
                SoundEngine.PlaySound(ShootSound, Projectile.position);
                OnShoot(Projectile.Center + ProjectileExtensions.PolarVector(Projectile.width / 3, Projectile.velocity.ToRotation()));
                Projectile.Kill();
            }
        }
    }
}
