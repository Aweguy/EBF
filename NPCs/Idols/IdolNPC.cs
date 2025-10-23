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
        private const float MaxRotation = 0.15f; // In radians
        private const float SpinSpeed = 0.5f; // In radians
        private float maxSpeed = 3f, accel = 0.1f;
        private bool isSpinning = false;
        private int rotationDirection = 1;
        private int textureFrame;
        protected int goreCount;

        public virtual SoundStyle IdolHitSound => SoundID.Item1;
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
            NPC.rotation = isSpinning 
                ? NPC.rotation + SpinSpeed * NPC.direction
                : MathHelper.Clamp(NPC.rotation + 0.01f * rotationDirection, -MaxRotation, MaxRotation);

            // Movement
            var dir = Vector2.Normalize(NPC.DirectionTo(Main.player[NPC.target].Center));
            NPC.velocity.X = Math.Clamp(NPC.velocity.X + dir.X * accel, -maxSpeed, maxSpeed);

            // Jumping
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.ai[0], ref NPC.ai[1]);
            if (NPC.collideY && NPC.oldVelocity.Y >= 0)
            {
                NPC.velocity.Y = Main.rand.NextBool(5) ? -10 : -5;
                if (!isSpinning)
                    rotationDirection = -rotationDirection;
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
