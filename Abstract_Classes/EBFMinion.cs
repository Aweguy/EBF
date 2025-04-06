using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using EBF.Extensions;
using Terraria.DataStructures;

namespace EBF.Abstract_Classes
{
    public abstract class EBFMinion : ModProjectile
    {
        private int attackTimer;
        private const float maxJumpHeight = 16f;
        private const float gravity = 0.2f;
        private bool isFlying = false;

        #region Properties
        /// <summary>
        /// How many ticks must elapse before the next attack.
        /// <para>Defaults to 60 (1 second).</para>
        /// </summary>
        public int AttackTime { get; set; } = 60;

        /// <summary>
        /// The range at which the minion notices a foe.
        /// <para>Defaults to 500.</para>
        /// </summary>
        public int DetectRange { get; set; } = 500;

        /// <summary>
        /// The range at which the minion will begin to shoot projectiles or do specific attacking actions. If the minion only uses contact damage, you can ignore this.
        /// <br>This does not affect how far away the minion searches for targets, use DetectRange for that.</br>
        /// <para>Defaults to 50.</para>
        /// </summary>
        public int AttackRange { get; set; } = 50;

        /// <summary>
        /// This property is true while the minion is in range to shoot projectiles or do attack actions. This does not relate to contact damage.
        /// </summary>
        public bool InAttackRange { get; private set; } = false;

        /// <summary>
        /// The distance the minion needs to be from the player in order to teleport.
        /// <para>Defaults to 1600.</para>
        /// </summary>
        public int TeleportDistance { get; set; } = 1600;

        /// <summary>
        /// This property determines if the minion is a grounded type or a hovering type.
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool UseHoverAI { get; set; } = false;

        /// <summary>
        /// This property determines how fast the minion walks or floats around.
        /// <para>Defaults to 5f.</para>
        /// </summary>
        public float MoveSpeed { get; set; } = 5f;

        /// <summary>
        /// This property lets the minion receive an effect when its cat toy weapon manages to hit something. 
        /// <br>Non-minions should not use this property, go to EBFCatToy.BoostDuration and ApplyBoost instead.</br>
        /// <para>Defaults to 0.</para>
        /// </summary>
        public int BoostTime { get; set; } = 0;
        
        /// <summary>
        /// A quick access check to see if the minion has a boost effect from its cat toy.
        /// </summary>
        public bool IsBoosted => BoostTime > 0;

        /// <summary>
        /// A reference to the NPC that the minion is currently targetting.
        /// </summary>
        public NPC Target => target;
        private NPC target = null;

        #endregion Properties

        #region Hooks
        /// <summary>
        /// When the minion is within attacking range (specified by the AttackRange property) and the attack cooldown is done (specified by the AttackSpeed property), this hook is called.
        /// </summary>
        public virtual void OnAttack(NPC target)
        { }
        public virtual void SetStaticDefaultsSafe()
        { }
        public virtual void SetDefaultsSafe()
        { }
        public virtual void OnSpawnSafe(IEntitySource source)
        { }
        public virtual void AISafe()
        { }
        #endregion Hooks

        #region Base Behavior
        public sealed override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;

            Main.projPet[Projectile.type] = true;
            SetStaticDefaultsSafe();
        }
        public sealed override void SetDefaults()
        {
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            SetDefaultsSafe();
        }
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
        public sealed override void OnSpawn(IEntitySource source)
        {
            attackTimer = AttackTime;
            isFlying = UseHoverAI;
            OnSpawnSafe(source);
        }
        public sealed override void AI()
        {
            if(BoostTime > 0)
            {
                BoostTime--;
            }

            Player player = Main.player[Projectile.owner];
            Vector2 idlePosition;
            if (isFlying)
            {
                idlePosition = player.Center - Vector2.UnitY * 32;
                PushOverlappingMinions();
            }
            else
            {
                idlePosition = player.Center + new Vector2((10 + Projectile.minionPos * 40) * -player.direction, 0);
                Projectile.velocity.Y += 0.2f; //Apply gravity
            }

            HandleTeleportToPlayer(idlePosition);

            //Locate and attack nearest target. (isFlying == UseHoverAI) prevents ground minions from targetting while flying.
            target = null;
            InAttackRange = false;
            if (isFlying == UseHoverAI && ProjectileExtensions.ClosestNPC(ref target, DetectRange, Projectile.position, ignoreTiles: isFlying))
            {
                HandleTargetLogic(target);
            }
            else
            {
                HandleIdleLogic(idlePosition);
                TryCancelGroundMinionFlying();
                NudgeStationaryHoverMinions();
            }

            //Slight lean towards movement
            Projectile.rotation = Projectile.velocity.X * 0.05f;

            AISafe();
        }

        private void PushOverlappingMinions()
        {
            float overlapVelocity = 0.04f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                //Check if both minions are owned by the same player, and if they're overlapping
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && other.ModProjectile is EBFMinion && (Projectile.position - other.position).Length() < Projectile.width)
                {
                    //Nudge the minion
                    Projectile.velocity.X += (Projectile.position.X > other.position.X) ? overlapVelocity : -overlapVelocity;
                    Projectile.velocity.Y += (Projectile.position.Y > other.position.Y) ? overlapVelocity : -overlapVelocity;
                }
            }
        }
        private void HandleTeleportToPlayer(Vector2 idlePosition)
        {
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            if (Main.myPlayer == Main.player[Projectile.owner].whoAmI && vectorToIdlePosition.Length() > 1600f)
            {
                // Infrequent events with drastic effects should only be run on the owner of the projectile, and a netUpdate should happen.
                Projectile.position = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }
        }
        private void HandleTargetLogic(NPC target)
        {
            //Attack enemy in range
            if ((target.Center - Projectile.Center).Length() < AttackRange)
            {
                InAttackRange = true;
                
                attackTimer--;
                if (attackTimer <= 0)
                {
                    OnAttack(target);
                    attackTimer = AttackTime;
                }

                //Stop in place
                if (isFlying)
                {
                    Projectile.velocity *= 0.95f;
                }
                else
                {
                    Projectile.velocity.X *= 0.95f;
                }
            }
            else 
            {
                //Move towards enemy
                if (isFlying)
                {
                    Projectile.velocity += Projectile.Center.DirectionTo(target.Center) * 0.33f * (1 + MoveSpeed * 0.05f);
                }
                else
                {
                    Projectile.velocity.X += Projectile.Center.DirectionTo(target.Center).X * 0.33f * (1 + MoveSpeed * 0.05f);
                    JumpTo(target.Center);
                }
            }

            Projectile.direction = Projectile.spriteDirection = Math.Sign(-Projectile.DirectionTo(target.position).X);
        }
        private void HandleIdleLogic(Vector2 idlePosition)
        {
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();

            //Attempt jump
            if(!isFlying && vectorToIdlePosition.Y < -200f)
            {
                JumpTo(idlePosition);
            }

            //The immediate range around the player
            if (isFlying && distanceToIdlePosition > 20f || !isFlying && distanceToIdlePosition > 0f)
            {
                float speed = MoveSpeed;
                float inertia = 20;

                if (distanceToIdlePosition > 400f)
                {
                    speed *= 2;
                    inertia *= 0.5f;
                    isFlying = true;
                }

                //Simple movement algorithm
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= speed;
                Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + vectorToIdlePosition.X) / inertia;

                if (isFlying)
                {
                    Projectile.velocity.Y = (Projectile.velocity.Y * (inertia - 1) + vectorToIdlePosition.Y) / inertia;
                }
            }

            Projectile.direction = Projectile.spriteDirection = Math.Sign(-Projectile.velocity.X);
        }
        protected void JumpTo(Vector2 targetPosition)
        {
            Tile tile = Framing.GetTileSafely(Projectile.Bottom + Vector2.UnitY * 8);
            if (tile.HasTile && Main.tileSolid[tile.TileType])
            {
                float distanceY = targetPosition.Y - Projectile.Center.Y;
                float jumpVelocityY = (float)Math.Sqrt(2 * gravity * Math.Abs(distanceY));

                Projectile.velocity.Y = -Math.Min(jumpVelocityY, maxJumpHeight);
            }
        }
        private void TryCancelGroundMinionFlying()
        {
            if (isFlying != UseHoverAI)
            {
                Player player = Main.player[Projectile.owner];
                if (player.velocity.Y == 0f && Collision.SolidTiles(player.position, player.width, player.height + 2))
                {
                    isFlying = false;
                }
            }
        }
        private void NudgeStationaryHoverMinions()
        {
            if (isFlying && Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity.X = -0.15f;
                Projectile.velocity.Y = -0.05f;
            }
        }

        #endregion Base Behavior
    }
}
