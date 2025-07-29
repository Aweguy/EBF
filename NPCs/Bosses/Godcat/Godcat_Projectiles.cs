using EBF.Abstract_Classes;
using EBF.EbfUtils;
using EBF.Gores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
                Projectile.rotation = EBFUtils.SlowRotation(Projectile.rotation, angleToTarget, turnSpeed);
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

    public class Godcat_LightDiamond : Godcat_BallProjectile
    {
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_LightDiamond";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 10;
            Projectile.height = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
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

                Projectile.HomeTowards(Owner, 0.33f, 12f);
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
        private Player Target => Main.player[Owner.target];
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

                //Dust
                if (Main.rand.NextBool(2))
                {
                    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                    dust.noGravity = true;
                }
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
            var vectorToPlayer = Target.Center - Projectile.Center;
            var spread = Main.rand.NextFloat(0.25f, 2.0f);
            Projectile.velocity = new Vector2(vectorToPlayer.X * spread * 0.009f, -14) + Target.velocity * 0.2f;

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
            Projectile.CreateExplosionEffect(EBFUtils.ExplosionSize.Large);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.CreateExplosionEffect(EBFUtils.ExplosionSize.Large);
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

    /// <summary>
    /// A slow moving invisible projectile which expands and rapidly spawns smoke within its hitbox.
    /// <br>ai[0] determines how big the smoke cloud becomes.</br>
    /// </summary>
    public class Destroyer_DarkBreath : ModProjectile
    {
        private ref float MaxSize => ref Projectile.ai[0];
        private Player HomingTarget => Main.player[(int)Projectile.ai[1]];
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.timeLeft = 60 * 5;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 4 == 0)
            {
                Projectile.velocity *= 0.97f;
                Projectile.HomeTowards(HomingTarget, 0.2f, 10f);
                SpawnGore();

                //Expand hitbox until max size
                if (Projectile.width < MaxSize)
                {
                    Projectile.ExpandHitboxTo(Projectile.width + 4, Projectile.height + 4);
                }
            }
        }
        private void SpawnGore()
        {
            var type = ModContent.GoreType<BigSmog>();
            var scale = 0.5f + (Projectile.width * 1.5f / MaxSize);
            for (int i = 0; i < 1 + Projectile.width / 200; i++)
            {
                var position = Projectile.position + Main.rand.NextVector2Square(0, Projectile.width * 0.8f);
                var velocity = Main.rand.NextVector2Square(-0.5f, 0.5f);
                var gore = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), position, velocity, type, scale);
                gore.rotation = MathHelper.PiOver2 * Main.rand.Next(1, 5);
                gore.alpha = 128;
            }
        }
    }

    public class Destroyer_FireWheel : ModProjectile
    {
        private struct BallInfo
        {
            public static int size = 50;
            public static Vector2 origin = new(25, 25);
            public Vector2 position;
            public Vector2 center;
            public Rectangle rect;
        }

        private BallInfo[] balls; // Used in collision and drawing
        private float radius = 1;
        private float rotationOffset;
        private Asset<Texture2D> texture;
        private NPC owner;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        private ref float StickTimer => ref Projectile.localAI[0];
        private ref float EasingTimer => ref Projectile.localAI[1];
        private ref float Amount => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;

            texture = ModContent.Request<Texture2D>($"Terraria/Images/Projectile_{ProjectileID.CultistBossFireBall}");
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            EBFUtils.ClosestNPC(ref owner, 128, Projectile.Center);

            //Default in case no ai is provided
            if (Amount == 0)
            {
                Amount = 10;
            }

            balls = new BallInfo[(int)Amount];
        }
        public override void AI()
        {
            //Radius
            if (EasingTimer < 1)
            {
                EasingTimer += 0.006f;
                radius = 500 * PolynomialEasing(EasingTimer);
            }

            //Rotation
            rotationOffset += 0.02f + EasingTimer * 0.01f;
            if (rotationOffset > MathF.Tau)
            {
                rotationOffset = 0;
            }

            //Sprite animation
            if (Main.GameUpdateCount % 4 == 0)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter > 3)
                {
                    Projectile.frameCounter = 0;
                }
            }

            //Stick to owner for a while
            StickTimer++;
            if (owner != null && owner.active)
            {
                if (StickTimer < 60)
                {
                    Projectile.Center = owner.Center;
                }
                else if (StickTimer == 60)
                {
                    Projectile.velocity = owner.DirectionTo(Main.player[owner.target].Center) * 3f;
                }
                else
                {
                    Projectile.velocity *= 1.01f;
                }
            }
            else
            {
                Projectile.velocity *= 1.01f;
            }

            //Update ball information
            balls = UpdateBalls();

            //Dust
            foreach (var ball in balls)
            {
                var dust = Dust.NewDustDirect(ball.position, BallInfo.size, BallInfo.size, DustID.Torch, 0, 0, 0, default, 1.5f);
                dust.noGravity = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            foreach (var ball in balls)
            {
                if (ball.rect.Intersects(targetHitbox))
                {
                    return true;
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!texture.IsLoaded)
                return false;

            foreach (var ball in balls)
            {
                Main.EntitySpriteDraw(
                    texture.Value,
                    ball.center - Main.screenPosition,
                    new Rectangle(0, BallInfo.size * Projectile.frameCounter, BallInfo.size, BallInfo.size),
                    lightColor,
                    0,
                    BallInfo.origin,
                    1f,
                    SpriteEffects.None
                    );
            }

            return false;
        }
        private BallInfo[] UpdateBalls()
        {
            byte i = 0;
            for (float theta = rotationOffset; theta < MathF.Tau + rotationOffset; theta += MathF.Tau / Amount)
            {
                balls[i].center = Projectile.Center + theta.ToRotationVector2() * radius;
                balls[i].position = balls[i].center - BallInfo.origin;
                balls[i].rect = new((int)balls[i].position.X, (int)balls[i].position.Y, BallInfo.size, BallInfo.size);
                i++;
            }

            return balls;
        }
        private static float PolynomialEasing(float x) => x * (4.18f + x * (-17.13f + x * (25.98f + x * -12.03f)));
    }
}
