using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    /// <summary>
    /// This class represents a standard Launcher, which charges up, sticks to the player and points at the cursor.
    /// <br>It is necessary for our modded guns to use this projectile, because they are unable to switch texture depending on whether RMB was pressed or not. 
    /// Instead, the launcher and corresponding sidearm are projectiles that simulate being a weapon.</br>
    /// </summary>
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
        /// How many ticks the weapon stays active after having fully charged.
        /// <para>Defaults to 0.</para>
        /// </summary>
        protected int ActiveTime { get; set; } = 0;

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
        /// This hook is called once the weapon is fully charged.
        /// </summary>
        /// <param name="barrelEnd">The approximate position of the launcher's barrel.</param>
        /// <param name="type">Reference to the bullet type in the player's inventory.</param>
        public virtual void OnShoot(Vector2 barrelEnd, int type) { }

        /// <summary>
        /// This hook is called while the weapon is fully charged.
        /// </summary>
        /// <param name="barrelEnd">The approximate position of the launcher's barrel.</param>
        /// <param name="type">Reference to the bullet type in the player's inventory.</param>
        public virtual void WhileShoot(Vector2 barrelEnd, int type) { }

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
            Main.player[Projectile.owner].ChangeDir(Projectile.direction);
        }
        private void HandleCharge(Player player)
        {
            if (charge == 0)
            {
                SoundEngine.PlaySound(ChargeSound, Projectile.position);
                OnChargeBegin();
            }

            if (charge < MaxCharge)
            {
                charge++;
                EnforceItemStay(player);
            }
            else
            {
                //Identify bullet type
                if (player.PickAmmo(player.HeldItem, out int type, out _, out _, out _, out _, true))
                {
                    //Get the barrel's estimated position
                    Vector2 barrelOffset = VectorUtils.Polar(Projectile.width / 3, Projectile.velocity.ToRotation());

                    //Run only once
                    if (Projectile.localAI[0] == 0)
                    {
                        Projectile.localAI[0] = 1;
                        SoundEngine.PlaySound(ShootSound, Projectile.position);
                        OnShoot(Projectile.Center + barrelOffset, type);
                    }

                    //Run every frame (Note that this method runs after OnShoot, in case it is relevant for you)
                    WhileShoot(Projectile.Center + barrelOffset, type);
                }

                //Check if the launcher should die or stay for some time
                ActiveTime--;
                if (ActiveTime <= 0)
                {
                    Projectile.Kill();
                }
                else
                {
                    EnforceItemStay(player);
                }
            }
        }
        private void EnforceItemStay(Player player)
        {
            if (player.itemTime < 2)
            {
                player.itemTime = 2;
                player.itemAnimation = 2;
                Projectile.timeLeft = 2;
            }
        }
    }
}
