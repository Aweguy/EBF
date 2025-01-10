using EBF.Extensions;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace EBF.Abstract_Classes
{
    public abstract class EBFChargeableArrow : ModProjectile
    {
        private bool isReleased = false;
        private int baseAiStyle;
        private float drawTime = 0;//The current charge value
        private bool giveTileCollision = false;//stores arrow tilecollision bool for later so it doesn't hit things while the player is drawing the arrow.
        private float baseVelocity; //Set this SetDefaults in each bow

        /// <summary>
        /// The maximum amount of charge an arrow can have, at which point draw time will stop increasing. Draw time starts at 0 and ticks up by 1 every update while an arrow exists.
        /// <br>If you wish to check if the arrow is fully charged, use the FullyCharged property instead.</br>
        /// </summary>
        protected int MaximumDrawTime { get; set; }//Set this SetDefaults in each bow

        /// <summary>
        /// The minimum time it takes before an arrow can be released. Draw time starts at 0 and ticks up by 1 every update while an arrow exists.
        /// <br>If the player releases their click earlier than this given time, the bow will keep charging until it meets this threshold, at which it will then shoot.</br>
        /// </summary>
        protected int MinimumDrawTime { get; set; }//Set this SetDefaults in each bow

        /// <summary>
        /// True when the arrow is fully charged.
        /// </summary>
        protected bool FullyCharged => (int)drawTime == MaximumDrawTime;

        /// <summary>
        /// How much the damage should be multiplied based on its charging percentage. The value cannot be set below 1.
        /// <br>Defaults to 2 times increase.</br>
        /// </summary>
        protected float DamageScale
        {
            get => damageScale;
            set
            {
                if (value < 1) damageScale = 1;
                else damageScale = value;
            }
        }
        private float damageScale = 2;

        /// <summary>
        /// How much the velocity should be multiplied based on its charging percentage. The value cannot be set below 1.
        /// <br>Defaults to 2 times increase.</br>
        /// </summary>
        protected float VelocityScale
        {
            get => velocityScale;
            set
            {
                if (value < 1) velocityScale = 1;
                else velocityScale = value;
            }
        }
        private float velocityScale = 2;

        /// <summary>
        /// Called once the projectile has been released from the weapon.
        /// </summary>
        public virtual void OnProjectileRelease() { }

        /// <summary>
        /// Called after the base class has finished PreAI().
        /// </summary>
        public virtual void PreAISafe() { }

        /// <summary>
        /// Called after the base class has finished OnSpawn().
        /// </summary>
        public virtual void OnSpawnSafe() { }

        public sealed override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            //Consume ammo
            Player player = Main.player[Projectile.owner];
            player.PickAmmo(player.HeldItem, out _, out _, out _, out _, out _);

            //Prevent arrow from acting while held
            Projectile.friendly = false;
            baseVelocity = Projectile.velocity.Length();
            baseAiStyle = Projectile.aiStyle;
            Projectile.aiStyle = 0;

            if (Projectile.tileCollide)
            {
                giveTileCollision = true;
                Projectile.tileCollide = false;
            }

            //Allow further customization
            OnSpawnSafe();
        }
        public override sealed bool PreAI()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return false;
            }
            if (isReleased) //Removing this will cause already fired arrows to return to the bow. Idk why, it's probably a reference type thing.
            {
                return true; //Use the projectile's actual AI.
            }

            Player player = Main.player[Projectile.owner];
            bool isHolding = player.channel || drawTime < MinimumDrawTime;

            if (isHolding)
            {
                HandleArrow(player);
                HandlePlayer(player);
                HandleDrawTime(player);
            }
            else
            {
                //Run only once
                if (Projectile.localAI[0] == 0)
                {
                    Projectile.localAI[0]++;
                    ReleaseProjectile(); //Return arrow stats & boost by drawtime
                }
            }

            //Allow further customization
            PreAISafe();

            return false;
        }
        private void HandleArrow(Player player)
        {
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            if (Main.myPlayer == Projectile.owner)
            {
                //Update velocity to face cursor
                Vector2 oldVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Normalize(Main.MouseWorld - playerCenter);

                if (oldVelocity != Projectile.velocity)
                {
                    Projectile.netUpdate = true;
                }
            }

            Vector2 drawOffset = ProjectileExtensions.PolarVector(28 - (8f * drawTime / MaximumDrawTime), Projectile.rotation - MathHelper.PiOver2);
            Projectile.Center = playerCenter + drawOffset; //the vector is a bandaid fix, we need to find the real reason the arrow is offset
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; //Accounting sprite facing up
            Projectile.timeLeft += Projectile.extraUpdates + 1;
        }
        private void HandlePlayer(Player player)
        {
            player.ChangeDir(Projectile.direction);
            
            //These checks exists so the arrow doesn't remove the bow's usetime
            if (player.itemTime < 2)
            {
                player.itemTime = 2;
            }
            if (player.itemAnimation < 2)
            {
                player.itemAnimation = 2;
            }

            //Using the projectile's rotation instead of velocity will break the player's hands, even if the bow sprite is correct.
            player.itemRotation = MathF.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
        }
        private void HandleDrawTime(Player player)
        {
            if (drawTime < MaximumDrawTime)
            {
                drawTime++;
                if ((int)drawTime == MaximumDrawTime) //cast to eliminate possible float precision error
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, player.position);
                }
            }
            else
            {
                //Light the tip of the arrow
                Vector2 offset = ProjectileExtensions.PolarVector(12, Projectile.rotation - MathHelper.PiOver2);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.AncientLight, Vector2.Zero);
                dust.noGravity = true;
            }
        }
        private void ReleaseProjectile()
        {
            isReleased = true;

            SoundEngine.PlaySound(SoundID.Item5, Projectile.position);

            //Calculate boosts from the arrow's draw time.
            float damageBoost = 1 + (damageScale - 1) * (drawTime / MaximumDrawTime);
            float velocityBoost = 1 + (velocityScale - 1) * (drawTime / MaximumDrawTime);

            Projectile.damage = (int)(Projectile.damage * damageBoost);
            Projectile.velocity *= baseVelocity * velocityBoost;

            //Restore projectile stats
            Projectile.friendly = true;
            Projectile.aiStyle = baseAiStyle;

            if (giveTileCollision)
            {
                Projectile.tileCollide = true;
            }

            //Allow further customization
            OnProjectileRelease();
        }
    }
}
