using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;

namespace EBF.NPCs.Idols
{
    public abstract class IdolNPC : ModNPC
    {
        private int rotationDirection = 1;
        private bool isSpinning = false;
        private float maxSpeed = 3f, accel = 0.1f;
        private int textureFrame;
        public int goreCount;

        public virtual SoundStyle IdolHitSound => SoundID.Item1;
        public virtual SoundStyle IdolJumpSound => SoundID.Item1;
        public virtual SoundStyle IdolBigJumpSound => SoundID.Item1;
        public virtual int HitDustID => DustID.WoodFurniture;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
            NPC.width = 48;
            NPC.height = 48;

            if (!Main.dedServ)
                NPC.HitSound = IdolHitSound;
        }
        public override void FindFrame(int frameHeight)
        {
            // Spawn with random frame from spritesheet
            if (NPC.localAI[0]++ == 0)
            {
                textureFrame = Main.rand.Next(3);
                NPC.frame.Y = textureFrame * frameHeight;
            }
        }
        public override void AI()
        {
            NPC.TargetClosest(true);
            NPC.spriteDirection = NPC.direction;

            // Rotation
            if (isSpinning)
            {
                NPC.rotation += MathHelper.ToRadians(30) * NPC.direction;
            }
            else
            {
                NPC.rotation += MathHelper.ToRadians(1) * rotationDirection;
                NPC.rotation = MathHelper.Clamp(NPC.rotation, MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
            }

            // Movement
            var dir = Vector2.Normalize(NPC.DirectionTo(Main.player[NPC.target].Center));
            NPC.velocity.X = Math.Clamp(NPC.velocity.X + dir.X * accel, -maxSpeed, maxSpeed);

            // Jumping
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.ai[0], ref NPC.ai[1]);
            if (NPC.collideY)
            {
                if (!isSpinning)
                    rotationDirection = -rotationDirection;

                if (Main.rand.NextBool(5))
                {
                    NPC.velocity.Y = -10f;
                    SoundEngine.PlaySound(IdolBigJumpSound, NPC.position);
                }
                else
                {
                    NPC.velocity.Y = -5f;
                    SoundEngine.PlaySound(IdolJumpSound, NPC.position);
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) => OnHit();
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) => OnHit();
        protected void OnHit()
        {
            for (int i = 0; i <= 5; i++)
                Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, HitDustID, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));

            if (NPC.life <= NPC.lifeMax * 0.25f && !isSpinning)
            {
                isSpinning = true;
                maxSpeed *= 2f;
                accel *= 2f;
            }
        }

        public override void OnKill()
        {
            for (int i = 0; i <= 20; i++)
                Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, HitDustID, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));

            for (int i = 0; i < goreCount; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * NPC.direction, Mod.Find<ModGore>($"{GetType().Name}{textureFrame}_Gore{i}").Type, 1f);
        }
    }
}
