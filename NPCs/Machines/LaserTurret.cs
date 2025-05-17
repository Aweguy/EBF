using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using System;

namespace EBF.NPCs.Machines
{
    public class LaserTurret : ModNPC
    {
        private Asset<Texture2D> baseTexture;
        private Asset<Texture2D> bodyTexture;
        private Asset<Texture2D> glowTexture;
        private Rectangle baseRect;
        private Vector2 targetPos;
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
            NPC.lifeMax = 400;
            NPC.value = 100;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;

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
            NPC.TargetClosest(); // only if not shooting. We don't want it to flip while laserbeaming.

            Player player = Main.player[NPC.target];
            targetPos = player.Center;

            if (Main.GameUpdateCount % 120 == 0 && Vector2.Distance(NPC.position, player.position) < 800)
            {
                Shoot(player);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!glowTexture.IsLoaded || !bodyTexture.IsLoaded || !baseTexture.IsLoaded)
                return false;

            //Draw base back
            baseRect.Y = 0;
            var position = NPC.Center + new Vector2(0, (NPC.height - baseRect.Height) / 2) - screenPos;
            var origin = baseRect.Size() * 0.5f;
            var flipX = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(baseTexture.Value, position, baseRect, drawColor, 0f, origin, 1f, flipX, 0);

            //Draw body
            position = NPC.Center + new Vector2(0, -10) - screenPos;
            origin = bodyTexture.Size() * 0.5f;
            var rotation = NPC.Center.AngleTo(targetPos) + MathHelper.Pi;
            var flipY = NPC.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(bodyTexture.Value, position, null, drawColor, rotation, origin, 1f, flipY, 0);

            //Draw glow
            var pulse = (float)Math.Abs(Math.Sin(Main.time * 0.02f));
            var color = new Color(pulse, pulse, pulse);
            spriteBatch.Draw(glowTexture.Value, position, null, color, rotation, origin, 1f, flipY, 0);

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
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos), ProjectileID.PhantasmalDeathray, NPC.damage, 3);
        }
    }
}
