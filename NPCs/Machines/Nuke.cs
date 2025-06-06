using EBF.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public abstract class Nuke : ModProjectile
    {
        protected bool inGround = false;

        public virtual void SetDefaultsSafe() { }
        public virtual void AISafe() { }
        public virtual bool OnTileCollideSafe(Vector2 oldVelocity) { return false; }
        public sealed override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.tileCollide = false;

            SetDefaultsSafe();
        }
        public sealed override void AI()
        {
            if (!inGround)
            {
                Projectile.frameCounter++;

                //Enable collision after a moment cuz we don't want it exploding immediately
                if (Projectile.frameCounter == 20)
                {
                    Projectile.tileCollide = true;
                }
                //Stage 1
                if (Projectile.frameCounter < 40)
                {
                    Projectile.velocity.Y -= 0.5f;
                }
                //Stage 2
                else if (Projectile.frameCounter > 40 && Projectile.frameCounter < 120)
                {
                    Projectile.velocity.X += Projectile.direction * 0.2f;
                    Projectile.velocity.Y += 0.25f;
                }
                //Stage 3
                else if (Projectile.frameCounter > 120)
                {
                    Projectile.velocity.X *= 0.98f;
                    Projectile.velocity.Y += 0.25f;
                }


                if (Projectile.velocity != Vector2.Zero)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                }
            }

            AISafe();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnTileCollideSafe(oldVelocity);

            if (!inGround)
            {
                Projectile.frameCounter = 0; // Reuse the timer for grounded logic
                Projectile.timeLeft = 120;
                inGround = true;
            }

            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}
