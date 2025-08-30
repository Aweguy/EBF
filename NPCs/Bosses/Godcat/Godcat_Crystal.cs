using EBF.NPCs.Machines;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    public abstract class Godcat_Crystal : ModNPC
    {
        private const float maxSpeed = 3f, accel = 0.5f;
        private const int preferredDistanceMin = 400, preferredDistanceMax = 600;
        protected virtual int AttackCooldown => 120;
        private ref float AttackTimer => ref NPC.localAI[0];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 16;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = 58;
            NPC.height = 62;
            NPC.lifeMax = 18000;
            NPC.defense = 35;
            NPC.damage = 40;
            NPC.knockBackResist = 0.5f;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.friendly = false;

            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.value = Item.buyPrice(gold: 5);
        }
        public override void FindFrame(int frameHeight)
        {
            // Idle frames
            NPC.frameCounter += 0.2f;
            if (NPC.frameCounter >= Main.npcFrameCount[Type])
            {
                NPC.frameCounter = 0;
            }
            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }
        public override void AI()
        {
            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;
            var player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                NPC.noGravity = false;
                return;
            }

            var dist = NPC.Distance(player.Center);
            Move(player, dist);
            PushOverlappingNPC();

            //Attacking
            AttackTimer++;
            if (AttackTimer > AttackCooldown && InPreferredDistance(dist))
            {
                Attack(player);
                AttackTimer = 0;
            }
        }
        private void Move(Player player, float dist)
        {
            if (dist < preferredDistanceMin)
            {
                //Move away from player
                NPC.velocity += NPC.DirectionFrom(player.position) * accel;
            }
            else if (dist > preferredDistanceMax)
            {
                //Move towards player
                NPC.velocity += NPC.DirectionTo(player.position) * accel;
            }

            Vector2.Clamp(NPC.velocity, Vector2.One * -maxSpeed, Vector2.One * maxSpeed);
            NPC.velocity *= 0.99f;
        }
        private static bool InPreferredDistance(float dist) => dist >= preferredDistanceMin && dist <= preferredDistanceMax;
        protected abstract void Attack(Player player);
        private void PushOverlappingNPC()
        {
            float overlapVelocity = 0.04f;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC other = Main.npc[i];
                if (i != NPC.whoAmI && other.active && (NPC.position - other.position).Length() < NPC.width)
                {
                    //Nudge the NPC away
                    NPC.velocity.X += (NPC.position.X > other.position.X) ? overlapVelocity : -overlapVelocity;
                    NPC.velocity.Y += (NPC.position.Y > other.position.Y) ? overlapVelocity : -overlapVelocity;
                }
            }
        }
    }
}
