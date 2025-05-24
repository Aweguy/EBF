using EBF.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public abstract class Turret : ModNPC
    {
        private Asset<Texture2D> baseTexture;
        private Asset<Texture2D> bodyTexture;
        private Asset<Texture2D> glowTexture;
        private Rectangle baseRect;
        private Vector2 originOffset = Vector2.UnitX * 12; // Adjusts the pivot point, so the turret rotates around the attachment and not its center.
        protected ref float IsShooting => ref NPC.ai[0]; // This value is read by Neon Valkyrie so she won't jump.
        protected ref float IsEnraged => ref NPC.ai[1]; // This is set to true by Neon Valkyrie in her second phase.
        
        public virtual void SetStaticDefaultsSafe() { }
        public virtual void SetDefaultsSafe() { }
        public virtual void AISafe() { }
        public virtual void OnKillSafe() { }
        public sealed override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            SetStaticDefaultsSafe();
        }
        public sealed override void SetDefaults()
        {
            NPC.value = 100;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0;

            baseTexture = ModContent.Request<Texture2D>("EBF/NPCs/Machines/NV_TurretBase");
            bodyTexture = ModContent.Request<Texture2D>(Texture);
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
            DrawOffsetY = bodyTexture.Height();

            SetDefaultsSafe();
        }
        public sealed override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = -MathHelper.PiOver2;
            baseRect = new Rectangle(0, 0, baseTexture.Width(), baseTexture.Height() / 2);
        }
        public sealed override void AI()
        {
            NPC.TargetClosest(false);

            //Flip direction based on rotation
            NPC.direction = Math.Sign(NPC.rotation.ToRotationVector2().X);
            if (NPC.direction == 0)
                NPC.direction = 1;

            //Move up animation after spawning
            if (DrawOffsetY > 0)
                DrawOffsetY--;

            AISafe();
        }
        public sealed override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!glowTexture.IsLoaded || !bodyTexture.IsLoaded || !baseTexture.IsLoaded)
                return false;

            //Draw base back
            baseRect.Y = 0;
            var position = NPC.Center + new Vector2(0, (baseRect.Height / 2) + DrawOffsetY) - screenPos;
            var origin = baseRect.Size() * 0.5f;
            var flipX = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(baseTexture.Value, position, baseRect, drawColor, 0f, origin, 1f, flipX, 0);

            //Draw body
            position = NPC.Center + new Vector2(0, -10 + DrawOffsetY) - screenPos - (originOffset * NPC.direction);
            origin = bodyTexture.Size() * 0.5f + originOffset;
            var realRotation = NPC.rotation + MathHelper.Pi;
            var flipY = NPC.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(bodyTexture.Value, position, null, drawColor, realRotation, origin, 1f, flipY, 0);

            //Draw glow
            var pulse = (float)Math.Abs(Math.Sin(Main.time * 0.02f));
            var color = new Color(pulse, pulse, pulse);
            spriteBatch.Draw(glowTexture.Value, position, null, color, realRotation, origin, 1f, flipY, 0);

            //Draw base front
            baseRect.Y += baseRect.Height;
            position = NPC.Center + new Vector2(0, (baseRect.Height / 2) + DrawOffsetY) - screenPos;
            origin = baseRect.Size() * 0.5f;
            spriteBatch.Draw(baseTexture.Value, position, baseRect, drawColor, 0f, origin, 1f, flipX, 0);

            return false;
        }
        public sealed override void OnKill()
        {
            CreateExplosionEffect();

            for (int i = 0; i < 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 4).RotatedByRandom(1f) + NPC.velocity, Mod.Find<ModGore>($"TurretBase_Gore0").Type, NPC.scale);
            
            OnKillSafe();
        }

        protected void LerpRotationToTarget(Player player, float lerpSpeed)
        {
            float angleToPlayer = NPC.DirectionTo(player.Center).ToRotation();
            float angleDiff = MathHelper.WrapAngle(angleToPlayer - NPC.rotation);
            NPC.rotation += angleDiff * lerpSpeed;
        }
        private void CreateExplosionEffect()
        {
            Dust dust;

            // Smoke Dust spawn
            for (int i = 0; i < 8; i++)
            {
                dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, Alpha: 100, Scale: 2f);
                dust.velocity += Vector2.Normalize(dust.position - NPC.Center) * 6;
            }
            // Fire Dust spawn
            for (int i = 0; i < 20; i++)
            {
                dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, Alpha: 100, newColor: Color.Yellow, Scale: Main.rand.NextFloat(1f, 2f));
                dust.velocity += Vector2.Normalize(dust.position - NPC.Center) * 2;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 3; g++)
            {
                Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, VectorUtils.Random(1.5f), Main.rand.Next(61, 64), Scale: 1f);
            }
        }
    }
}
