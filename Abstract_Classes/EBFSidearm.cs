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
    /// This class represents a standard sidearm to a launcher weapon, which sticks to the player and points at the cursor.
    /// <br>It is necessary for our modded guns to use this projectile, because they are unable to switch texture depending on whether RMB was pressed or not. 
    /// Instead, the sidearm and corresponding launcher are projectiles that simulate being a weapon.</br>
    /// </summary>
    public abstract class EBFSidearm : ModProjectile
    {
        /// <summary>
        /// The sound this item makes when shooting. Set this to an existing <see cref="SoundID"/> entry or assign to a new <see cref="SoundStyle"/> for a custom sound.
        /// <br/> For example <c>ShootSound = SoundID.Item11;</c> can be used for a bullet being fired.
        /// <para/> Defaults to Item11 (musket ball sound).
        /// </summary>
        protected SoundStyle ShootSound { get; set; } = SoundID.Item11;

        /// <summary>
        /// How many ticks the weapon stays active after being used.
        /// <para>Defaults to 0.</para>
        /// </summary>
        protected int ActiveTime { get; set; } = 0;

        /// <summary>
        /// This hook is called while the weapon is active.
        /// </summary>
        public virtual void WhileShoot(Vector2 barrelEnd, int type) { }

        /// <summary>
        /// This hook is called once when the weapon is active.
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
            HandleShoot(player);
            HandleTimeLeft(player);

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

            Projectile.position += VectorUtils.Polar(Projectile.width / 2, Projectile.velocity.ToRotation());
        }
        private void HandleShoot(Player player) 
        {
            //Identify bullet type
            if (player.PickAmmo(player.HeldItem, out int type, out _, out _, out _, out _, true))
            {
                //Run only once
                if (Projectile.localAI[0] == 0)
                {
                    Projectile.localAI[0] = 1;
                    SoundEngine.PlaySound(ShootSound, Projectile.position);
                    OnShoot(Projectile.Center + VectorUtils.Polar(Projectile.width / 4, Projectile.velocity.ToRotation()), type);
                }

                //Run every frame (Note that this method runs after OnShoot, in case it is relevant for you)
                WhileShoot(Projectile.Center + VectorUtils.Polar(Projectile.width / 4, Projectile.velocity.ToRotation()), type);
            }
        }
        private void HandleTimeLeft(Player player)
        {
            if(ActiveTime != 0)
            {
                ActiveTime--;

                //Keep item active
                if (player.itemTime < 2)
                {
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    Projectile.timeLeft = 2;
                }
            }

            Projectile.timeLeft = player.itemTime + 1;
        }
    }
}
