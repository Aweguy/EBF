using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.GameContent;
using System;
using Terraria.DataStructures;

namespace EBF.Abstract_Classes
{
    /// <summary>
    /// This base class handles most logic required for a death ray.
    /// <br>Originally sourced from FargoWiltaSouls public github repos.</br>
    /// </summary>
    public abstract class EBFDeathRay : ModProjectile
    {
        private Texture2D beamTexture;
        protected float hitboxModifier = 1f;
        protected int drawDistance = 3000;
        protected int maxTime = 90;
        protected Vector3 lightColor = Vector3.One;
        protected ref float Timer => ref Projectile.localAI[0];
        protected ref float BeamLength => ref Projectile.localAI[1];
        protected Vector2 BeamEnd => Projectile.Center + Projectile.velocity * BeamLength;

        public virtual void SetStaticDefaultsSafe() { }
        public virtual void SetDefaultsSafe() { }
        public virtual void OnSpawnSafe(IEntitySource source) { }
        public virtual void AISafe() { }
        public sealed override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = drawDistance;
            SetStaticDefaultsSafe();
        }
        public sealed override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.hostile = true;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false; //only beam start has the hitbox, rest don't care
            Projectile.timeLeft = maxTime;

            SetDefaultsSafe();
        }
        public sealed override void OnSpawn(IEntitySource source)
        {
            beamTexture = TextureAssets.Projectile[Type].Value;
            OnSpawnSafe(source);
        }
        public sealed override void AI()
        {
            // Sine wave scaling for pulsating beam effect (enter and exit scale effect)
            Timer += 1f;
            Projectile.scale = Math.Min(MathF.Sin(Timer * MathF.PI / maxTime) * 3f, 1);

            // Align beam to velocity
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Grow beam length from zero to max (prevents drawing many segments when scale is tiny)
            BeamLength = MathHelper.Lerp(BeamLength, drawDistance, 0.5f);
            CastLights();
            AISafe();
        }
        public sealed override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 50) * 0.95f;
        public sealed override bool PreDraw(ref Color lightColor)
        {
            // Don't draw if the projectile isn't moving.
            if (Projectile.velocity == Vector2.Zero || Projectile.velocity.HasNaNs() || beamTexture == null)
            {
                return false;
            }

            // Load textures and frames.
            Rectangle beamBegFrame = beamTexture.Frame(verticalFrames: 3, frameY: 0);
            Rectangle beamMidFrame = beamTexture.Frame(verticalFrames: 3, frameY: 1);
            Rectangle beamEndFrame = beamTexture.Frame(verticalFrames: 3, frameY: 2);

            // Adjust color for transparency
            Color drawColor = Projectile.GetAlpha(lightColor);
            drawColor = Color.Lerp(drawColor, Color.Transparent, Projectile.alpha / 255f);

            // Draw the start of the beam
            Main.EntitySpriteDraw(
                beamTexture,
                Projectile.Center - Main.screenPosition,
                beamBegFrame,
                drawColor,
                Projectile.rotation,
                beamBegFrame.Size() / 2,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // Starting point (after the beginning piece) for drawing the repeating middle part
            Vector2 drawPos = Projectile.Center + Projectile.velocity * (beamMidFrame.Height * Projectile.scale / 2f);

            // Length to draw (in pixels) along the beam, subtracted by the visual size of the start and end pieces
            float remainingLength = BeamLength - (beamBegFrame.Height / 2 + beamEndFrame.Height) * Projectile.scale;

            if (remainingLength > 0f)
            {
                // Draw the middle beam segments repeatedly until we fill the needed length
                for (float drawnLength = 0f; drawnLength + 1f < remainingLength; drawnLength += beamMidFrame.Height * Projectile.scale)
                {
                    // If this next segment would go past the needed length, trim it
                    if (remainingLength - drawnLength < beamMidFrame.Height)
                    {
                        beamMidFrame.Height = (int)(remainingLength - drawnLength);
                    }

                    Main.EntitySpriteDraw(
                        beamTexture,
                        drawPos - Main.screenPosition,
                        beamMidFrame,
                        drawColor,
                        Projectile.rotation,
                        new Vector2(beamMidFrame.Width / 2, 0),
                        Projectile.scale,
                        SpriteEffects.None,
                        0
                    );

                    drawPos += Projectile.velocity * beamMidFrame.Height * Projectile.scale;
                }
            }

            // Draw the end of the beam
            Main.EntitySpriteDraw(
                beamTexture,
                drawPos - Main.screenPosition,
                beamEndFrame,
                drawColor,
                Projectile.rotation,
                new Vector2(beamEndFrame.Width / 2, 0),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
        public sealed override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            var width = Projectile.width * Projectile.scale;
            Utils.PlotTileLine(Projectile.Center, BeamEnd, width, DelegateMethods.CutTiles);
        }
        public sealed override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }

            var lineWidth = beamTexture.Width * 0.66f * Projectile.scale * hitboxModifier;
            var _ = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, BeamEnd, lineWidth, ref _))
            {
                return true;
            }
            return false;
        }
        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = lightColor;
            Utils.PlotTileLine(Projectile.Center, BeamEnd, beamTexture.Width, DelegateMethods.CastLight);
        }
    }
}