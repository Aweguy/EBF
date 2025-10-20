using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public abstract class NukeNPC : ModProjectile
    {
        protected bool inGround = false;

        public virtual void SetDefaultsSafe() { }
        public virtual void SetStaticDefaultsSafe() { }
        public virtual void AISafe() { }
        public virtual bool OnTileCollideSafe(Vector2 oldVelocity) { return false; }
        public sealed override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
            SetStaticDefaultsSafe();
        }
        public sealed override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.hide = true;

            SetDefaultsSafe();
        }
        public sealed override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, Projectile.position);
            var player = Main.LocalPlayer;
            var dir = Math.Sign(Projectile.DirectionTo(player.position).X);
            if (dir == 0)
            {
                dir = -1;
            }
            Projectile.direction = dir;
            Projectile.velocity.X = dir * 0.01f; // Prevents vanilla from automatically resetting direction to 1
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
                if (Projectile.frameCounter < 80)
                {
                    Projectile.velocity.Y -= 0.25f;
                    CreateThrusterDust();
                }
                //Stage 2
                else if (Projectile.frameCounter > 80 && Projectile.frameCounter < 160)
                {
                    Projectile.velocity.X += Projectile.direction * 0.2f;
                    Projectile.velocity.Y += 0.25f;
                    CreateThrusterDust();
                }
                //Stage 3
                else if (Projectile.frameCounter > 160)
                {
                    Projectile.velocity.X *= 0.98f;
                    Projectile.velocity.Y += 0.25f;
                }
                //Face moving direction
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
        private void CreateThrusterDust()
        {
            var pos = Projectile.position + new Vector2(0, Projectile.height * 0.75f).RotatedBy(Projectile.rotation);
            Dust.NewDust(pos, Projectile.width, Projectile.width, DustID.Torch, Scale: 2f);
            Dust.NewDust(pos, Projectile.width, Projectile.width, DustID.Smoke, Scale: 2f);
        }
    }
}
