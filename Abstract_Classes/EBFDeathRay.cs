using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    /// <summary>
    /// This base class handles most logic required for a death ray.
    /// <br>Originally sourced from FargoWiltaSouls public github repos.</br>
    /// </summary>
    public abstract class EBFDeathRay : ModProjectile
    {
        private Texture2D beamTexture;
        private float timer = 0f;
        private float beamLength = 0f;
        protected float hitboxModifier = 1f;
        protected int drawDistance = 3000;
        protected int maxTime = 90;
        protected virtual Vector3 LightColor => Vector3.One;
        protected Vector2 BeamEnd => Projectile.Center + Projectile.velocity * beamLength;

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
            // Enter and exit scale effect
            timer += 1f;
            Projectile.scale = Math.Min(MathF.Sin(timer * MathF.PI / maxTime) * 3f, 1);

            // Align beam to velocity
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Grow beam length from zero to max (prevents drawing many segments when scale is tiny)
            beamLength = MathHelper.Lerp(beamLength, drawDistance, 0.5f);
            CastLights();
            AISafe();
        }
        public sealed override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 50) * 0.95f;
        public sealed override bool PreDraw(ref Color lightColor)
        {
            // Don't draw if not moving / invalid texture
            if (Projectile.velocity == Vector2.Zero || Projectile.velocity.HasNaNs() || beamTexture == null)
                return false;

            // Prepare frames and color
            Rectangle beamBegFrame = beamTexture.Frame(verticalFrames: 3, frameY: 0);
            Rectangle beamMidFrame = beamTexture.Frame(verticalFrames: 3, frameY: 1);
            Rectangle beamEndFrame = beamTexture.Frame(verticalFrames: 3, frameY: 2);

            Color drawColor = Projectile.GetAlpha(lightColor);
            drawColor = Color.Lerp(drawColor, Color.Transparent, Projectile.alpha / 255f);

            // Use Utils.DrawLaser. Draw positions must be world coordinates minus screen position
            // because the rest of your drawing used EntitySpriteDraw with screen-subtracted coords.
            Vector2 startPos = Projectile.Center - Main.screenPosition;
            Vector2 endPos = BeamEnd - Main.screenPosition;
            Vector2 scale = new(Projectile.scale, Projectile.scale);

            // Local function to get laser line frame info, passed to Utils.DrawLaser
            void GetLaserLineFrame(int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame,
                out float distanceCovered, out Rectangle frame, out Vector2 origin, out Color color)
            {
                // Default values
                distanceCovered = 0f;
                frame = Rectangle.Empty;
                origin = Vector2.Zero;
                color = drawColor;

                switch (stage)
                {
                    case 0: // head
                        frame = beamBegFrame;
                        distanceCovered = frame.Height;
                        origin = frame.Size() / 2f;
                        return;
                    case 1: // middle (repeated tile)
                        // Use lastFrame to keep continuity if needed. Use full tile height normally.
                        frame = beamMidFrame;
                        distanceCovered = frame.Height;
                        // origin at top center so tiled segments draw correctly
                        origin = new Vector2(frame.Width / 2f, 0f);
                        return;
                    case 2: // tail
                        frame = beamEndFrame;
                        distanceCovered = frame.Height;
                        origin = new Vector2(frame.Width / 2f, 0f);
                        return;
                }
            }

            Utils.DrawLaser(Main.spriteBatch, beamTexture, startPos, endPos, scale, GetLaserLineFrame);

            // Skip default drawing
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
                return true;

            var lineWidth = beamTexture.Width * 0.66f * Projectile.scale * hitboxModifier;
            var _ = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, BeamEnd, lineWidth, ref _))
                return true;
            
            return false;
        }
        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = LightColor;
            Utils.PlotTileLine(Projectile.Center, BeamEnd, beamTexture.Width, DelegateMethods.CastLight);
        }
    }
}