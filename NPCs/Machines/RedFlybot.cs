using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public class RedFlybot : FlybotNPC
    {
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky, // Spawn conditions
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.RedFlybot") // Description
            ]);
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Ensure all flybots don't shoot at the same time
            NPC.localAI[1] = Main.GameUpdateCount;
        }
        public override void AISafe()
        {
            NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            Move(player);

            //Shoot twice
            var shootFrame = (Main.GameUpdateCount + NPC.localAI[1]) % 80;
            if ((shootFrame == 0 || shootFrame == 6) && Vector2.Distance(NPC.position, player.position) < 600)
            {
                Shoot(player);
            }

            //Reduce cannon recoil
            for (int i = 0; i < 2; i++)
                cannonOffsets[i] *= 0.9f;
        }

        private void Shoot(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item158, NPC.Center);

            //Create projectile
            var velocity = NPC.DirectionTo(player.position) * 14;
            var type = ModContent.ProjectileType<RedFlybot_Laser>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3);
            proj.friendly = false;
            proj.hostile = true;

            //Recoil
            CannonIndexToUse = CannonIndexToUse == 0 ? 1 : 0;
            cannonOffsets[(int)CannonIndexToUse] = -velocity;

            //Dust
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.RedTorch, velocity.X, velocity.Y, Scale: 2.5f);
                dust.noGravity = true;
            }
        }
    }
    public class RedFlybot_Laser : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item89, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FireworkFountain_Red);
            }
        }
    }
}
