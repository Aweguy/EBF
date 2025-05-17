using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using EBF.Items.Summon;

namespace EBF.NPCs.Machines
{
    public class RedFlybot : ModNPC
    {
        private Asset<Texture2D> bodyTexture;
        private Asset<Texture2D> cannonTexture;
        private const float maxSpeedH = 4f, maxSpeedV = 3f, accelH = 1f, accelV = 1f;
        private Vector2[] cannonOffsets = new Vector2[2];
        private Vector2 CannonPosA => NPC.Center + new Vector2(16 * NPC.direction, 10) + cannonOffsets[0];
        private Vector2 CannonPosB => NPC.Center + new Vector2(-16 * NPC.direction, 10) + cannonOffsets[1];
        private ref float CannonIndexToUse => ref NPC.localAI[0];
        public override string Texture => "EBF/Items/Summon/RiotShield_RedFlybotMinion";
        public override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
            NPC.width = 68;
            NPC.height = 54;
            NPC.damage = 30;
            NPC.defense = 18;
            NPC.lifeMax = 400;
            NPC.value = 100;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;

            bodyTexture = ModContent.Request<Texture2D>(Texture);
            cannonTexture = ModContent.Request<Texture2D>(Texture.Replace("Minion", "_Cannon"));
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Ensure all flybots don't shoot at the same time
            NPC.localAI[1] = Main.GameUpdateCount; 
        }
        public override void AI()
        {
            NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            Move(player);

            //Shoot twice
            var shootFrame = (Main.GameUpdateCount + NPC.localAI[1]) % 80;
            if ((shootFrame == 0 || shootFrame == 6) && Vector2.Distance(NPC.position, player.position) < 600)
            {
                Shoot(player);
            }

            //Reduce cannon recoil
            for (int i = 0; i < 2; i++)
                cannonOffsets[i] *= 0.9f;
        }
        public override void FindFrame(int frameHeight)
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

        private void Move(Player player)
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
            if(Math.Sign(dir.Y) != Math.Sign(NPC.velocity.Y))
            {
                NPC.velocity.Y *= 0.95f;
            }

            if (NPC.wet)
            {
                NPC.velocity.Y -= 0.5f;
            }
        }
        private void Shoot(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item158, NPC.Center);

            //Create projectile
            var velocity = NPC.DirectionTo(player.position) * 14;
            var type = ModContent.ProjectileType<RedFlybotLaser>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3);
            proj.friendly = false;
            proj.hostile = true;

            //Recoil
            CannonIndexToUse = CannonIndexToUse == 0 ? 1 : 0;
            cannonOffsets[(int)CannonIndexToUse] = -velocity;

            //Dust
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RedTorch, velocity.X, velocity.Y, Scale: 2.5f);
                dust.noGravity = true;
            }
        }
    }
}
