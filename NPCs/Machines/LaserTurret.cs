using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System;
using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Terraria.GameContent.Bestiary;

namespace EBF.NPCs.Machines
{
    public class LaserTurret : TurretNPC
    {
        private ref float Timer => ref NPC.localAI[0];
        private ref float AttackChoice => ref NPC.localAI[1];
        private ref float BallsFired => ref NPC.localAI[2];
        public override void SetStaticDefaultsSafe()
        {
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/LaserTurret_Preview",
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 70;
            NPC.height = 56;
            NPC.damage = 30;
            NPC.defense = 18;
            NPC.lifeMax = 2000;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, // Spawn conditions
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.LaserTurret") // Description
            ]);
        }
        public override void AISafe()
        {
            Player player = Main.player[NPC.target];
            Timer++;

            if (IsShooting)
            {
                TurnTowardsTarget(player);
                if (Timer >= 90)
                {
                    IsShooting = false;
                }
                return;
            }

            if (Timer < 240)
            {
                LerpRotationToTarget(player, 0.1f);
                return;
            }

            if (Timer < 300)
            {
                if(AttackChoice == 0)
                {
                    ShootBalls();
                    return;
                }

                if(Timer == 240)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath58, NPC.Center);
                }

                ChargeUpDust();
                RotateAheadOfTarget(player);
                return;
            }

            if (Timer < 340)
            {
                ChargeUpDust();
                return;
            }

            Timer = 0;
            if (Vector2.Distance(NPC.position, player.position) < 1000)
            {
                ShootLaser();
                IsShooting = true;
                AttackChoice = Main.rand.Next(2);
            }
        }
        public override void OnKillSafe()
        {
            for (int i = 0; i < 4; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 4).RotatedByRandom(1f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore{i}").Type, NPC.scale);
        }
        private void ShootBalls()
        {
            if(Main.GameUpdateCount % 10 == 0)
            {
                //Shoot projectile
                var speed = 12 + Main.rand.NextFloat(-1f, 1f);
                var vel = NPC.rotation.ToRotationVector2().RotatedByRandom(1f) * speed;
                var type = ModContent.ProjectileType<LaserTurret_Ball>();
                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, vel, type, NPC.damage / 2, 3);

                SoundEngine.PlaySound(SoundID.Item157, NPC.Center);

                var maxBallCount = 3;
                BallsFired++;
                if(BallsFired >= maxBallCount)
                {
                    Timer = 0;
                    BallsFired = 0;
                    AttackChoice = Main.rand.Next(2);
                }
            }
        }
        private void ShootLaser()
        {
            var velocity = NPC.rotation.ToRotationVector2();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<LaserTurret_Laser>(), NPC.damage, 3, -1, NPC.target);

            var sound = SoundID.Zombie104; // Moon Lord deathray sound
            sound.Pitch = 1.4f;
            sound.Volume = 0.3f;
            SoundEngine.PlaySound(sound, NPC.Center);
        }
        private void ChargeUpDust()
        {
            var pos = NPC.Center + NPC.rotation.ToRotationVector2().RotatedByRandom(0.5f) * 64;
            var vel = pos.DirectionTo(NPC.Center);
            var dust = Dust.NewDustPerfect(pos, DustID.AmberBolt, vel, 0, default, 1.25f);
            dust.noGravity = true;
        }
        private void RotateAheadOfTarget(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            float baseAngle = toPlayer.ToRotation();

            // Step 1: Determine orbital direction (signed angular velocity)
            float tangentialDir = MathF.Sign(Vector2.Dot(player.velocity, toPlayer.RotatedBy(MathHelper.PiOver2)));

            // Step 2: Offset angle in tangential direction
            float angleOffset = MathHelper.Pi / 4f; // degrees
            float targetAngle = baseAngle + tangentialDir * angleOffset;

            // Step 3: Smooth rotation toward targetAngle
            float angleDiff = MathHelper.WrapAngle(targetAngle - NPC.rotation);
            NPC.rotation += angleDiff * 0.1f;
        }
        private void TurnTowardsTarget(Player player)
        {
            var currentAngle = NPC.rotation;
            var targetAngle = NPC.AngleTo(player.Center);

            // Normalize the angle difference to [-π, π]
            float delta = MathHelper.WrapAngle(targetAngle - currentAngle);

            // Turn toward the target
            float turnSpeed = 0.005f;
            float newAngle = currentAngle + MathF.Sign(delta) * turnSpeed;

            NPC.rotation = newAngle;
        }
    }
    public class LaserTurret_Ball : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, TorchID.Yellow);
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            dust.noGravity = true;

            //Homing
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            Vector2 toTarget = Projectile.DirectionTo(target.Center);
            float newRotation = EbfUtils.EBFUtils.SlowRotation(Projectile.velocity.ToRotation(), toTarget.ToRotation(), 0.33f);
            Projectile.velocity = newRotation.ToRotationVector2() * Projectile.velocity.Length();
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            }
        }
    }
    public class LaserTurret_Laser : EBFDeathRay
    {
        private NPC owner;
        private Player target;
        protected override Vector3 LightColor => Color.Orange.ToVector3();
        public override void OnSpawnSafe(IEntitySource source)
        {
            target = Main.player[(int)Projectile.ai[0]];
            EBFUtils.ClosestNPC(ref owner, 400, Projectile.position, true);
        }
        public override void AISafe()
        {
            if (owner == null || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = owner.Center + Projectile.velocity * 36;
            TurnTowardsTarget();
        }

        private void TurnTowardsTarget()
        {
            var currentAngle = Projectile.velocity.ToRotation();
            var targetAngle = Projectile.AngleTo(target.Center);

            // Normalize the angle difference to [-π, π]
            float delta = MathHelper.WrapAngle(targetAngle - currentAngle);

            // Turn toward the target
            float turnSpeed = 0.005f;
            float newAngle = currentAngle + MathF.Sign(delta) * turnSpeed;

            Projectile.velocity = newAngle.ToRotationVector2();
        }
    }
}
