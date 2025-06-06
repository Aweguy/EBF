using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using EBF.Extensions;

namespace EBF.NPCs.Machines
{
    /// <summary>
    /// This class represents the stand that holds any given Nuke. It manages a timer and launches its nuke towards a target when the time has elapsed.
    /// <br>The nuke itself does not exist while this stand is idle, the nuke is instead created as a seperate entity upon launch.</br>
    /// </summary>
    public class NukeStand : ModNPC
    {
        private bool hasLaunched = false;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
        }
        public override void SetDefaults()
        {
            NPC.width = 62;
            NPC.height = 92;
            NPC.defense = 18;
            NPC.lifeMax = 3000;

            NPC.value = 100;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0;
        }
        public override void OnSpawn(IEntitySource source)
        {
            // Use default nuke if none has been provided
            if (NPC.ai[0] < 1)
            {
                NPC.ai[0] = ModContent.ProjectileType<NuclearBomb>();
            }
        }
        public override void AI()
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 60 * 1)
            {
                Launch();
                hasLaunched = true;
            }
            if (NPC.frameCounter >= 60 * 2)
            {
                NPC.StrikeInstantKill();
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = hasLaunched ? frameHeight : 0;
        }
        public override void OnKill()
        {
            var velocity = Vector2.Zero;

            if (!hasLaunched)
            {
                NPC.CreateExplosionEffect();
                SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.position);
                velocity = new Vector2(0, -2).RotatedByRandom(1f);
            }

            //Gore.NewGore(NPC.GetSource_Death(), NPC.Center, velocity, Mod.Find<ModGore>($"{Name}_Gore").Type, NPC.scale);
        }
        private void Launch()
        {
            var type = (int)NPC.ai[0]; // Given by the npc that created the stand.
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Top, Vector2.Zero, type, 120, 5f);
        }
    }
}
