using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using EBF.Extensions;
using Microsoft.Xna.Framework.Graphics;

namespace EBF.NPCs.Machines
{
    /// <summary>
    /// This class represents the stand that holds any given Nuke. It manages a timer and launches its nuke towards a target when the time has elapsed.
    /// <br>The nuke itself does not exist while this stand is idle, the nuke is instead created as a seperate entity upon launch.</br>
    /// </summary>
    public class NukeStand : ModNPC
    {
        private bool hasLaunched = false;
        private Texture2D glowmaskTexture;
        private Color signalColor = Color.Red;
        private Vector2 SignalPosition => NPC.position + new Vector2(NPC.width * 0.25f, NPC.height * 0.75f);
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

            NPC.value = 0;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0;
            glowmaskTexture = ModContent.Request<Texture2D>(Texture + "_Glowmask").Value;
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
            //Countdown
            NPC.frameCounter++;
            if (NPC.frameCounter == 60 * 10)
            {
                SoundEngine.PlaySound(SoundID.Item75, NPC.position);
                signalColor = Color.Yellow;
            }
            if (NPC.frameCounter == 60 * 15)
            {
                SoundEngine.PlaySound(SoundID.Item75, NPC.position);
                signalColor = Color.Green;
            }
            //Launch
            if (NPC.frameCounter == 60 * 20)
            {
                Launch();
                hasLaunched = true;
            }
            //Break stand
            if (NPC.frameCounter >= 60 * 21)
            {
                NPC.StrikeInstantKill();
            }

            Lighting.AddLight(SignalPosition, signalColor.ToVector3());
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = hasLaunched ? frameHeight : 0;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.spriteBatch.Draw(
                    glowmaskTexture,
                    NPC.Center - Main.screenPosition,
                    new Rectangle(0, 0, glowmaskTexture.Width, glowmaskTexture.Height),
                    signalColor,
                    NPC.rotation,
                    glowmaskTexture.Size() / 2,
                    NPC.scale,
                    SpriteEffects.None,
                    0);
        }
        public override void OnKill()
        {
            var velocity = Vector2.Zero;

            if (!hasLaunched)
            {
                //Explode
                NPC.CreateExplosionEffect();
                SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.position);
                velocity = new Vector2(0, -2).RotatedByRandom(1f);
            }

            //Break apart
            for (int i = 0; i < 3; i++)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, velocity, Mod.Find<ModGore>($"{Name}_Gore{i}").Type, NPC.scale);
            }
        }
        private void Launch()
        {
            var type = (int)NPC.ai[0]; // Nuke type is given by the npc that created the stand.
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - new Vector2(0, 2), Vector2.Zero, type, 120, 5f);
        }
    }
}
