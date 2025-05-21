using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public abstract class Flybot : ModNPC
    {
        protected Asset<Texture2D> bodyTexture;
        protected Asset<Texture2D> cannonTexture;
        protected float maxSpeedH = 1f, maxSpeedV = 1f, accelH = 1f, accelV = 1f;
        protected Vector2[] cannonOffsets = new Vector2[2];
        protected Vector2 CannonPosA => NPC.Center + new Vector2(16 * NPC.direction, 10) + cannonOffsets[0];
        protected Vector2 CannonPosB => NPC.Center + new Vector2(-16 * NPC.direction, 10) + cannonOffsets[1];
        protected ref float CannonIndexToUse => ref NPC.localAI[0];

        public virtual void SetStaticDefaultsSafe() { }
        public virtual void SetDefaultsSafe() { }
        public sealed override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            Main.npcFrameCount[Type] = 3;
            SetStaticDefaultsSafe();
        }
        public sealed override void SetDefaults()
        {
            NPC.width = 68;
            NPC.height = 54;
            NPC.value = 100;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;

            bodyTexture = ModContent.Request<Texture2D>(Texture);
            cannonTexture = ModContent.Request<Texture2D>(Texture + "_Cannon");
            SetDefaultsSafe();
        }
        public sealed override void FindFrame(int frameHeight)
        {
            //Animation
            if (Main.GameUpdateCount % 4 == 0)
            {
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > 2 * frameHeight)
                {
                    NPC.frame.Y = 0 * frameHeight;
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!cannonTexture.IsLoaded)
                return false;

            var player = Main.player[NPC.target];

            //Draw back cannon
            var position = CannonPosA - screenPos;
            var rotation = CannonPosA.AngleTo(player.Center);
            var origin = cannonTexture.Value.Size() * 0.5f;
            spriteBatch.Draw(cannonTexture.Value, position, null, drawColor, rotation, origin, 1f, SpriteEffects.None, 0);

            //Draw body
            position = NPC.Center - screenPos;
            origin = NPC.frame.Size() * 0.5f;
            var flip = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(bodyTexture.Value, position, NPC.frame, drawColor, 0f, origin, 1f, flip, 0);

            //Draw front cannon
            position = CannonPosB - screenPos;
            rotation = CannonPosB.AngleTo(player.Center);
            origin = cannonTexture.Value.Size() * 0.5f;
            spriteBatch.Draw(cannonTexture.Value, position, null, drawColor, rotation, origin, 1f, SpriteEffects.None, 0);

            return false;
        }
        protected void Move(Player player)
        {
            //Upon collision, flip velocity and add extra if low.
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
                if (Math.Abs(NPC.velocity.X) < 1f)
                {
                    NPC.velocity.X = Math.Sign(NPC.velocity.X) * 2;
                }
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
                if (Math.Abs(NPC.velocity.Y) < 1f)
                {
                    NPC.velocity.Y = Math.Sign(NPC.velocity.Y);
                }
            }

            //Add force towards target
            Vector2 dir = NPC.DirectionTo(player.position);
            NPC.velocity.X = Math.Clamp(NPC.velocity.X + dir.X * accelH * 0.05f, -maxSpeedH, maxSpeedH);
            NPC.velocity.Y = Math.Clamp(NPC.velocity.Y + dir.Y * accelV * 0.02f, -maxSpeedV, maxSpeedV);

            //Add drag if flying away from target
            if (Math.Sign(dir.X) != Math.Sign(NPC.velocity.X))
            {
                NPC.velocity.X *= 0.95f;
            }
            if (Math.Sign(dir.Y) != Math.Sign(NPC.velocity.Y))
            {
                NPC.velocity.Y *= 0.95f;
            }

            if (NPC.wet)
            {
                NPC.velocity.Y -= 0.5f;
            }
        }
    }
}
