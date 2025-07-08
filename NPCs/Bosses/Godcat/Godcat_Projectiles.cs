using Microsoft.Xna.Framework;
using EBF.Extensions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

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
}
