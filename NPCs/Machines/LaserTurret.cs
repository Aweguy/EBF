using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using System;
using EBF.Abstract_Classes;
using EBF.Extensions;

namespace EBF.NPCs.Machines
{
    public class LaserTurret : ModNPC
    {
        private Asset<Texture2D> baseTexture;
        private Asset<Texture2D> bodyTexture;
        private Asset<Texture2D> glowTexture;
        private Rectangle baseRect;
        private ref float IsLasering => ref NPC.ai[0]; //This value is also read by Neon Valkyrie so it doesn't do BS maneuvers.
        private ref float Timer => ref NPC.localAI[0];
        public override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 56;
            NPC.damage = 30;
            NPC.defense = 18;
            NPC.lifeMax = 2000;
            NPC.value = 100;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0;

            baseTexture = ModContent.Request<Texture2D>("EBF/NPCs/Machines/NV_TurretBase");
            bodyTexture = ModContent.Request<Texture2D>(Texture);
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
        public override void OnSpawn(IEntitySource source)
        {
            baseRect = new Rectangle(0, 0, baseTexture.Width(), baseTexture.Height() / 2);
        }
        public override void AI()
        {
            NPC.TargetClosest(false); // only if not shooting. We don't want it to flip while laserbeaming.
            
            //Flip direction based on rotation
            NPC.direction = Math.Sign(-NPC.rotation.ToRotationVector2().X);
            if (NPC.direction == 0)
                NPC.direction = 1;

            Player player = Main.player[NPC.target];

            //Handle rotation
            if (IsLasering == 1)
            {
                TurnTowardsTarget(player);
                Timer++;
                if (Timer >= 90)
                {
                    IsLasering = 0;
                    Timer = 0;
                }
            }
            else
            {
                RotateAheadOfTarget(player);
            }
            
            //Handle shooting
            if (Main.GameUpdateCount % 250 == 0 && Vector2.Distance(NPC.position, player.position) < 800)
            {
                Shoot(player);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!glowTexture.IsLoaded || !bodyTexture.IsLoaded || !baseTexture.IsLoaded)
                return false;

            // Adjusts the pivot point, so the turret rotates around the attachment and not its center.
            var funnyOffset = Vector2.UnitX * 12; 

            //Draw base back
            baseRect.Y = 0;
            var position = NPC.Center + new Vector2(0, (NPC.height - baseRect.Height) / 2) - screenPos;
            var origin = baseRect.Size() * 0.5f;
            var flipX = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(baseTexture.Value, position, baseRect, drawColor, 0f, origin, 1f, flipX, 0);

            //Draw body
            position = NPC.Center + new Vector2(0, -10) - screenPos - (funnyOffset * NPC.direction);
            origin = bodyTexture.Size() * 0.5f + funnyOffset;
            var flipY = NPC.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(bodyTexture.Value, position, null, drawColor, NPC.rotation, origin, 1f, flipY, 0);

            //Draw glow
            var pulse = (float)Math.Abs(Math.Sin(Main.time * 0.02f));
            var color = new Color(pulse, pulse, pulse);
            spriteBatch.Draw(glowTexture.Value, position, null, color, NPC.rotation, origin, 1f, flipY, 0);

            //Draw base front
            baseRect.Y += baseRect.Height;
            position = NPC.Center + new Vector2(0, (NPC.height - baseRect.Height) / 2) - screenPos;
            origin = baseRect.Size() * 0.5f;
            spriteBatch.Draw(baseTexture.Value, position, baseRect, drawColor, 0f, origin, 1f, flipX, 0);

            return false;
        }
        private void Shoot(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item158, NPC.Center);
            var velocity = (NPC.rotation + MathHelper.Pi).ToRotationVector2();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<LaserTurret_Laser>(), NPC.damage, 3, -1, NPC.target);
            IsLasering = 1;
        }
        private void RotateAheadOfTarget(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            float baseAngle = toPlayer.ToRotation();

            // Step 1: Determine orbital direction (signed angular velocity)
            float tangentialDir = MathF.Sign(Vector2.Dot(player.velocity, toPlayer.RotatedBy(MathHelper.PiOver2)));

            // Step 2: Offset angle in tangential direction
            float angleOffset = MathHelper.Pi / 3f; // degrees
            float targetAngle = baseAngle + tangentialDir * angleOffset + MathHelper.Pi;

            // Step 3: Smooth rotation toward targetAngle
            float angleDiff = MathHelper.WrapAngle(targetAngle - NPC.rotation);
            NPC.rotation += angleDiff * 0.1f;
        }
        private void TurnTowardsTarget(Player player)
        {
            var currentAngle = NPC.rotation - MathHelper.Pi;
            var targetAngle = NPC.AngleTo(player.Center);

            // Normalize the angle difference to [-π, π]
            float delta = MathHelper.WrapAngle(targetAngle - currentAngle);

            // Turn toward the target
            float turnSpeed = 0.005f;
            float newAngle = currentAngle + MathF.Sign(delta) * turnSpeed;

            NPC.rotation = newAngle + MathHelper.Pi;
        }
    }

    public class LaserTurret_Laser : EBFDeathRay
    {
        private NPC owner;
        private Player target;
        public override void OnSpawn(IEntitySource source)
        {
            target = Main.player[(int)Projectile.ai[0]];
            ProjectileExtensions.ClosestNPC(ref owner, 400, Projectile.position, true);
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
