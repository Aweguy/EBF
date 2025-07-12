using Microsoft.Xna.Framework;
using EBF.Extensions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;
using Terraria.Audio;
using EBF.Abstract_Classes;
using System;

namespace EBF.NPCs.Bosses.Godcat
{
    public enum GodcatBallTypes : byte { LightSmall, LightBig, DarkSmall, DarkBig };

    /// <summary>
    /// This is a projectile that delays the spawning of another projectile.
    /// It saves us the trouble of managing timers on the spawning actor.
    /// </summary>
    public class DelayedProjectile : ModProjectile
    {
        private bool friendly;
        private bool hostile;
        public override bool ShouldUpdatePosition() => false;
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible
        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.timeLeft = 60; // 1 second delay by default
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            // Save the friendly and hostile flags so we can restore to the real projectile
            // Not done in OnSpawn, because that would set the flags before we modify the created projectile from the actor
            if (Projectile.frameCounter++ == 0)
            {
                friendly = Projectile.friendly;
                hostile = Projectile.hostile;
                Projectile.friendly = false;
                Projectile.hostile = false;
            }

            if (Projectile.timeLeft <= 1)
            {
                var type = (int)Projectile.ai[0];
                var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                proj.friendly = friendly;
                proj.hostile = hostile;

                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor) => false; // Don't draw this 
    }

    /// <summary>
    /// Variation of Light Blade that behaves differently to the one casted by heaven's gate.
    /// </summary>
    public class Godcat_LightBlade : ModProjectile
    {
        private const float turnSpeed = 0.5f;
        private bool animate = true;
        private float speed;
        private Player Target => Main.player[(int)Projectile.ai[0]];
        protected virtual int DustType => DustID.AncientLight;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 240;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override string Texture => "EBF/Items/Melee/HeavensGate_LightBlade";
        public override bool? CanHitNPC(NPC target) => false;
        public override bool? CanDamage() => !animate;
        public override bool ShouldUpdatePosition() => !animate;
        public override void OnSpawn(IEntitySource source)
        {
            speed = Projectile.velocity.Length();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void AI()
        {
            if (animate)
            {
                Animate();
            }
            else if (Projectile.timeLeft < 60)
            {
                animate = true;
            }
            else if (Target != null)
            {
                speed *= 1.01f;

                //Slight homing behavior while flying
                var angleToTarget = Projectile.AngleTo(Target.Center) + MathHelper.PiOver2;
                Projectile.rotation = ProjectileExtensions.SlowRotation(Projectile.rotation, angleToTarget, turnSpeed);
                Projectile.velocity = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * speed;
            }

            //Create dust
            if (Main.rand.NextBool(3))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustType);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            var velocity = Vector2.Normalize(Projectile.oldVelocity);
            var position = Projectile.position + velocity * 16f;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(position, Projectile.width, Projectile.height, DustType, velocity.X, velocity.Y);
                dust.noGravity = true;
            }
        }
        private void Animate()
        {
            //Advance frames every third tick
            if (Main.GameUpdateCount % 3 != 0)
                return;

            Projectile.frame++;
            switch (Projectile.frame)
            {
                case 4:
                    animate = false;
                    Projectile.netUpdate = true;
                    break;
                case 11:
                    Projectile.Kill();
                    break;
            }
        }
    }

    /// <summary>
    /// Copy of Godcat_LightBlade, but using dark texture.
    /// </summary>
    public class Godcat_DarkBlade : Godcat_LightBlade
    {
        protected override int DustType => DustID.RedTorch;
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_DarkBlade";
    }

    /// <summary>
    /// A large projectile which flies in a straight path, and explodes into smaller projectiles that home toward the caster
    /// <br>ai[0] determines size and color, use GodcatBallType enum if you don't want to guess.</br>
    /// <br>ai[1] is the homing target after the ball has broken apart.</br>
    /// <br>ai[2] is handled internally, it's used to check if the ball should return.</br>
    /// </summary>
    public class Godcat_ReturnBall : ModProjectile
    {
        private int dustType = DustID.AncientLight;
        private NPC Owner => Main.npc[(int)Projectile.ai[1]]; //Projectile.owner must be -1, otherwise it doesn't deal damage to the player. So we will pass the owner through ai
        private bool IsMiniVariant => Projectile.ai[2] == 1;
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_BallProjectile";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (int)Projectile.ai[0];
            if (Projectile.frame == 1 || Projectile.frame == 3)
            {
                Projectile.ExpandHitboxTo(32, 32);
            }
            if (Projectile.frame > 1)
            {
                dustType = DustID.RedTorch;
            }

            if (IsMiniVariant)
            {
                Projectile.timeLeft = 200;
            }
        }
        public override void AI()
        {
            if (IsMiniVariant)
            {
                if (Main.rand.NextBool(2))
                {
                    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                    dust.noGravity = true;
                }

                Projectile.HomeTowards(Owner, 0.3f);
                if (Projectile.Distance(Owner.Center) < 32)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (!IsMiniVariant)
            {
                //Replace projectile with a bunch of mini variants, with a bit of angle variation
                var amount = 8;
                var spread = 1f;
                var ballType = Projectile.ai[0] - 1; //Same color, but smaller version
                for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
                {
                    var velocity = Projectile.velocity.RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.position, velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, ballType, Owner.whoAmI, 1);
                }
            }

            //Create dust
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
            }
        }
    }

    /// <summary>
    /// A ball projectile which accelerates forward and changes angle over time.
    /// <br>ai[0] determines whether it should be light or dark, and if it should be big or small.
    /// Use the GodcatBallTypes enum if you don't want to guess which value means what.</br>
    /// <br>ai[1] determines the turning speed.</br>
    /// </summary>
    public class Godcat_TurningBall : ModProjectile
    {
        private int dustType = DustID.AncientLight;
        private ref float TurnRate => ref Projectile.ai[1];
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_BallProjectile";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.height = 16;
            Projectile.width = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (int)Projectile.ai[0];
            if (Projectile.frame == 1 || Projectile.frame == 3)
            {
                Projectile.ExpandHitboxTo(32, 32);
            }
            if (Projectile.frame > 1)
            {
                dustType = DustID.RedTorch;
            }
        }
        public override void AI()
        {
            TurnRate *= 1.002f;
            Projectile.velocity = Projectile.velocity.RotatedBy(TurnRate) * 1.005f;
            if (Main.rand.NextBool(2))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.oldVelocity.X, Projectile.oldVelocity.Y);
            }
        }
    }

    /// <summary>
    /// Simple ball projectile which accelerates forward.
    /// <br>ai[0] determines whether it should be light or dark, and if it should be big or small.
    /// Use the GodcatBallTypes enum if you don't want to guess which value means what.</br>
    /// </summary>
    public class Godcat_BallProjectile : ModProjectile
    {
        private int dustType = DustID.AncientLight;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.height = 16;
            Projectile.width = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (int)Projectile.ai[0];
            if (Projectile.frame == 1 || Projectile.frame == 3)
            {
                Projectile.ExpandHitboxTo(32, 32);
            }
            if (Projectile.frame > 1)
            {
                dustType = DustID.RedTorch;
            }
        }
        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.noGravity = true;
            }

            Projectile.velocity *= 1.005f;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
            }
        }
    }

    public class Creator_Thunderball : ModProjectile
    {
        protected Vector2 positionalOffset;
        private Vector2 PreferredPosition => Owner.Center + positionalOffset;
        private ref float ActivationTime => ref Projectile.ai[0];
        private bool DrawsBehindNpcs => Projectile.ai[1] == 1;
        private NPC Owner => Main.npc[(int)Projectile.ai[2]];
        public override string Texture => "EBF/NPCs/Machines/LaserTurret_Ball";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.hide = true;
        }
        public override bool ShouldUpdatePosition() => Projectile.frameCounter >= ActivationTime;
        public override void OnSpawn(IEntitySource source)
        {
            positionalOffset = Projectile.velocity * 128;
        }
        public override void AI()
        {
            //Behavior
            Projectile.frameCounter++;
            if (Projectile.frameCounter < ActivationTime)
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, PreferredPosition, 0.1f);
            }
            else if (Projectile.frameCounter == ActivationTime)
            {
                Launch();
                Projectile.tileCollide = true;
            }
            else if (Projectile.frameCounter > ActivationTime)
            {
                //Gravity
                Projectile.velocity.Y += 0.2f;
            }

            //Dust
            if (Main.rand.NextBool(2))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                dust.noGravity = true;
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (DrawsBehindNpcs)
            {
                behindNPCs.Add(index);
            }
            else
            {
                overPlayers.Add(index);
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.CreateExplosionEffect();
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.CreateExplosionEffect();
            Projectile.ExpandHitboxTo(64, 64);
            Projectile.Damage();
        }
        private void Launch()
        {
            var vectorToPlayer = Main.LocalPlayer.Center - Projectile.Center;
            Projectile.velocity = new Vector2(vectorToPlayer.X * 0.009f * Main.rand.NextFloat(0.25f, 2.0f), -14);

            SoundEngine.PlaySound(SoundID.Item39, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            }
        }
    }

    public class Creator_HugeThunderball : Creator_Thunderball
    {
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_Creator_HugeThunderball";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 128;
            Projectile.height = 128;
        }
        public override void OnSpawn(IEntitySource source)
        {
            positionalOffset = Projectile.velocity * 220;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.CreateExplosionEffect(Extensions.Utils.ExplosionSize.Large);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.CreateExplosionEffect(Extensions.Utils.ExplosionSize.Large);
            Projectile.ExpandHitboxTo(256, 256);
            Projectile.Damage();
        }
    }

    public class Creator_HolyDeathray : EBFDeathRay
    {
        private NPC Owner => Main.npc[(int)Projectile.ai[0]];
        private Vector2 OwnerBarrelPos => Owner.Center + new Vector2(80 * Owner.direction, -16);
        protected override Vector3 LightColor => Color.White.ToVector3();
        public override void AISafe()
        {
            if (Owner == null || !Owner.active)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = OwnerBarrelPos;
        }
    }

    public class Destroyer_DarkHomingBall : ModProjectile
    {
        private Player homingTarget;
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
        }
        public override void OnSpawn(IEntitySource source)
        {
            homingTarget = Main.player[(int)Projectile.ai[0]];
        }
        public override void AI()
        {
            Projectile.rotation += 0.1f;

            //Homing
            var success = Projectile.HomeTowards(homingTarget, 0.4f, 11f);
            if (!success)
            {
                Projectile.Kill();
            }

            //Dust
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
            dust.noGravity = true;
        }
        public override void OnKill(int timeLeft)
        {
            //Shoot out the outer ring of balls
            var speed = 6f;
            var type = ModContent.ProjectileType<Godcat_BallProjectile>();

            for (float theta = Projectile.rotation; theta < MathF.Tau + Projectile.rotation; theta += MathF.Tau / 16)
            {
                var pos = Projectile.Center + theta.ToRotationVector2() * 20;
                var velocity = theta.ToRotationVector2() * speed;
                Projectile.NewProjectile(Projectile.GetSource_Death(), pos, velocity, type, Projectile.damage, 3f, -1, (float)GodcatBallTypes.DarkSmall);
            }

            //Extra effects
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
            }
        }
    }
}
