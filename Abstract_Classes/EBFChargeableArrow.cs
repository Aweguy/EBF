using EBF.Extensions;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace EBF.Abstract_Classes
{
    public abstract class EBFChargeableArrow : ModProjectile
    {
        private Projectile arrow = null;
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
        /// Called after the base class has finished OnKill().
        /// </summary>
        /// <param name="projectile">The projectile that was fired from the bow. This projectile is different from the one held in the bow.
        /// <br>Use this parameter instead of the main Projectile property.</br></param>
        public virtual void OnKillSafe(Projectile projectile) { }

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

            //Get stats from bow
            Player player = Main.player[Projectile.owner];
            player.PickAmmo(player.HeldItem, out int ammo, out _, out int damage, out float knockback, out _);

            //Apply stats from bow to the projectile that is launched on release
            arrow = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ammo, damage, knockback, Projectile.owner);
            arrow.penetrate = Projectile.penetrate;
            arrow.friendly = false;
            arrow.localNPCHitCooldown = Projectile.localNPCHitCooldown;
            arrow.usesLocalNPCImmunity = Projectile.usesLocalNPCImmunity;
            baseVelocity = player.inventory[player.selectedItem].shootSpeed;

            //Prevent arrow from colliding with tiles while held
            if (arrow.tileCollide)
            {
                giveTileCollision = true;
                arrow.tileCollide = false;
            }

            //Allow further customization
            OnSpawnSafe();
        }
        public override sealed bool PreAI()
        {
            if (Main.netMode == NetmodeID.Server || arrow == null)
            {
                return false;
            }

            Player player = Main.player[Projectile.owner];
            bool isHolding = player.channel || drawTime < MinimumDrawTime;

            if (isHolding)
            {
                Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
                if (Main.myPlayer == Projectile.owner)
                {
                    //Update velocity
                    Vector2 oldVelocity = Projectile.velocity;
                    Projectile.velocity = Vector2.Normalize(Main.MouseWorld - playerCenter);

                    if (oldVelocity != Projectile.velocity)
                    {
                        Projectile.netUpdate = true;
                    }
                }

                //Lock arrow to player
                Projectile.position = playerCenter - Projectile.Size / 2;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; //Accounting sprite facing up


                UpdatePlayer(player);
                UpdateArrow();
                HandleTimer(player);
            }
            else
            {
                Projectile.Kill();
            }

            //Allow further customization
            PreAISafe();

            return false;
        }
        public override sealed void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item5, Projectile.position);

            //Calculate boosts from the arrow's draw time.
            float damageBoost = 1 + (damageScale - 1) * (drawTime / MaximumDrawTime);
            float velocityBoost = 1 + (velocityScale - 1) * (drawTime / MaximumDrawTime);

            arrow.damage = (int)(arrow.damage * damageBoost);
            arrow.velocity = Projectile.velocity * baseVelocity * velocityBoost;
            arrow.extraUpdates = Projectile.extraUpdates;
            arrow.friendly = true;

            if (arrow != null && giveTileCollision)
            {
                arrow.tileCollide = true;
            }

            //Allow further customization
            OnKillSafe(arrow);
        }
        private void HandleTimer(Player player)
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
                Vector2 offset = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 4;
                Dust dust = Dust.NewDustPerfect(arrow.Center + offset, DustID.AncientLight, Vector2.Zero);
                dust.noGravity = true;
            }
        }
        private void UpdateArrow()
        {
            float drawOffset = 8f * drawTime / MaximumDrawTime;
            arrow.Center = Projectile.Center + ProjectileExtensions.PolarVector(36 - drawOffset, Projectile.rotation - MathHelper.PiOver2);
            arrow.rotation = Projectile.rotation;
            arrow.velocity = Projectile.velocity;
            arrow.timeLeft += arrow.extraUpdates + 1;
        }
        private void UpdatePlayer(Player player)
        {
            player.ChangeDir(Projectile.direction);
            player.heldProj = arrow.whoAmI;
            
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
    }
}
