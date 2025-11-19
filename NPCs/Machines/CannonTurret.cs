using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public class CannonTurret : TurretNPC
    {
        private ref float Timer => ref NPC.localAI[0];
        private ref float ShotsFired => ref NPC.localAI[1];
        public override void SetStaticDefaultsSafe()
        {
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/CannonTurret_Preview",
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 104;
            NPC.height = 46;
            NPC.damage = 30;
            NPC.defense = 18;
            NPC.lifeMax = 2000;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, // Spawn conditions
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.CannonTurret") // Description
            ]);
        }
        public override void AISafe()
        {
            Player player = Main.player[NPC.target];
            Timer++;

            LerpRotationToTarget(player, 0.1f);

            if (Timer > 240)
            {
                IsShooting = true;
                Shoot();
            }
        }
        public override void OnKillSafe()
        {
            for (int i = 0; i < 4; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 4).RotatedByRandom(1f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore{i}").Type, NPC.scale);
        }

        private void Shoot()
        {
            if (Main.GameUpdateCount % 30 == 0)
            {
                //Shoot projectile
                var speed = 24;
                var vel = NPC.rotation.ToRotationVector2() * speed;
                var type = ProjectileID.CannonballHostile;
                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, vel, type, NPC.damage / 2, 3);
                proj.timeLeft = 40;
                proj.light = 0.8f;

                SoundEngine.PlaySound(SoundID.Item11, NPC.Center);

                var maxShotCount = 2;
                ShotsFired++;
                if (ShotsFired >= maxShotCount)
                {
                    Timer = 0;
                    ShotsFired = 0;
                    IsShooting = false;
                }
            }
        }
    }
}
