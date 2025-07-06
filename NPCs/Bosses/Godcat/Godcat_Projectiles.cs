using Microsoft.Xna.Framework;
using EBF.Extensions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace EBF.NPCs.Bosses.Godcat
{
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
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            var velocity = Vector2.Normalize(Projectile.oldVelocity);
            var position = Projectile.position + velocity * 16f;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(position, Projectile.width, Projectile.height, DustID.AncientLight, velocity.X, velocity.Y);
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
    /// A large projectile which flies in a straight path, and explodes into smaller projectiles that home toward the caster
    /// </summary>
    public class Godcat_LightReturnBall : ModProjectile
    {
        private bool IsMiniVariant => Projectile.ai[1] == 1;
        private NPC Owner => Main.npc[(int)Projectile.ai[0]]; //Projectile.owner must be -1, otherwise it doesn't deal damage to the player. So we will pass the owner through ai
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void OnSpawn(IEntitySource source)
        {
            if (IsMiniVariant)
            {
                Projectile.scale = 0.8f;
                Projectile.timeLeft = 200;
            }
            else
            {
                Projectile.scale = 1.2f;
            }
        }
        public override void AI()
        {
            if (IsMiniVariant)
            {
                if (Main.rand.NextBool(2))
                {
                    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
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
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
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
                for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
                {
                    var velocity = Projectile.velocity.RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.position, velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Owner.whoAmI, 1);
                }
            }

            //Create dust
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
            }
        }
    }

    /// <summary>
    /// Basic ball projectile that flies forward
    /// </summary>
    public class Godcat_LightBall : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 360;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
            }
        }
    }


    /// <summary>
    /// Copy of Godcat_LightBlade, but using dark texture.
    /// </summary>
    public class Godcat_DarkBlade : ModProjectile
    {
        private const float turnSpeed = 0.5f;
        private bool animate = true;
        private float speed;
        private Player Target => Main.player[(int)Projectile.ai[0]];
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
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            var velocity = Vector2.Normalize(Projectile.oldVelocity);
            var position = Projectile.position + velocity * 16f;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(position, Projectile.width, Projectile.height, DustID.AncientLight, velocity.X, velocity.Y);
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
    /// Copy of Godcat_LightReturnBall, but using dark texture.
    /// </summary>
    public class Godcat_DarkReturnBall : ModProjectile
    {
        private bool IsMiniVariant => Projectile.ai[1] == 1;
        private NPC Owner => Main.npc[(int)Projectile.ai[0]]; //Projectile.owner must be -1, otherwise it doesn't deal damage to the player. So we will pass the owner through ai
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_DarkBall_Big";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void OnSpawn(IEntitySource source)
        {
            if (IsMiniVariant)
            {
                Projectile.scale = 0.8f;
                Projectile.timeLeft = 200;
            }
            else
            {
                Projectile.scale = 1.2f;
            }
        }
        public override void AI()
        {
            if (IsMiniVariant)
            {
                if (Main.rand.NextBool(2))
                {
                    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
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
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
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
                for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
                {
                    var velocity = Projectile.velocity.RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.position, velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Owner.whoAmI, 1);
                }
            }

            //Create dust
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
            }
        }
    }

    /// <summary>
    /// Copy of Godcat_LightBall, but using dark texture.
    /// </summary>
    public class Godcat_DarkBall : ModProjectile
    {
        public override string Texture => "EBF/NPCs/Bosses/Godcat/Godcat_DarkBall_Small";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 360;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
            }
        }
    }

}
