using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public class CannonTurret : Turret
    {
        private ref float Timer => ref NPC.localAI[0];
        private ref float ShotsFired => ref NPC.localAI[1];
        public override void SetDefaultsSafe()
        {
            NPC.width = 104;
            NPC.height = 46;
            NPC.damage = 30;
            NPC.defense = 18;
            NPC.lifeMax = 2000;
        }
        public override void AISafe()
        {
            Player player = Main.player[NPC.target];
            Timer++;

            LerpRotationToTarget(player, 0.1f);

            if(Timer > 240)
            {
                IsShooting = 1;
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

                SoundEngine.PlaySound(SoundID.Item11, NPC.Center);

                var maxShotCount = 2;
                ShotsFired++;
                if (ShotsFired >= maxShotCount)
                {
                    Timer = 0;
                    ShotsFired = 0;
                    IsShooting = 0;
                }
            }
        }
    }
}
