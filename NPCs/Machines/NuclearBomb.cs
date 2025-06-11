using EBF.Extensions;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public class NuclearBomb : Nuke
    {
        private Texture2D glowmaskTexture;
        private Vector2 shakeDirection = Vector2.UnitX * 1; //Increase the multiplier to make the shaking more intense
        private const float diggingDepth = 48; //How far the missile is placed into the ground upon hitting it
        private const int explosionSize = 1600; //The hitbox size of the explosion
        private ref float GlowmaskOpacity => ref Projectile.localAI[0];

        public override void SetDefaultsSafe()
        {
            Projectile.width = 46;
            Projectile.height = 92;
            glowmaskTexture = ModContent.Request<Texture2D>(Texture + "_Glowmask").Value;
        }
        public override void AISafe()
        {
            if (inGround && Main.GameUpdateCount % 2 == 0)
            {
                //Shake
                Projectile.Center += shakeDirection;
                shakeDirection.X = -shakeDirection.X;

                shakeDirection *= 1.05f;
                Projectile.scale *= 1.01f;

                //Increase glow
                if (GlowmaskOpacity < 1)
                {
                    GlowmaskOpacity += 0.01f;
                }
            }
        }
        public override bool OnTileCollideSafe(Vector2 oldVelocity)
        {
            if (!inGround)
            {
                //Embed projectile into ground
                Projectile.position += Vector2.Normalize(oldVelocity) * diggingDepth;
                Projectile.velocity = Vector2.Zero;
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.position);
            }

            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            if (inGround)
            {
                Main.spriteBatch.Draw(
                    glowmaskTexture,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, 0, glowmaskTexture.Width, glowmaskTexture.Height),
                    Color.White * GlowmaskOpacity,
                    Projectile.rotation,
                    glowmaskTexture.Size() / 2,
                    Projectile.scale,
                    SpriteEffects.None,
                    0);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);

            //Explode
            ProjectileExtensions.ExpandHitboxBy(Projectile, explosionSize, explosionSize);
            Projectile.CreateExplosionEffect(Extensions.Utils.ExplosionSize.Nuclear);
            Projectile.Damage();
        }
    }
}
