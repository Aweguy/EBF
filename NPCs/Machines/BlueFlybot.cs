using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Bestiary;

namespace EBF.NPCs.Machines
{
    public class BlueFlybot : FlybotNPC
    {
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky, // Spawn conditions
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.BlueFlybot") // Description
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

            //Shoot five bubbles each 150 ticks
            var shootFrame = (Main.GameUpdateCount + NPC.localAI[1]) % 150;
            if ((shootFrame > 0 && shootFrame <= 20 && shootFrame % 4 == 0) && Vector2.Distance(NPC.position, player.position) < 600)
            {
                Shoot(player);
            }

            //Reduce cannon recoil
            for (int i = 0; i < 2; i++)
                cannonOffsets[i] *= 0.9f;
        }
        
        private void Shoot(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item85, NPC.position);

            //Create projectile
            var velocity = NPC.DirectionTo(player.position) * Main.rand.NextFloat(11, 13);
            var type = ModContent.ProjectileType<BlueFlybot_Bubble>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3);

            //Recoil
            CannonIndexToUse = CannonIndexToUse == 0 ? 1 : 0;
            cannonOffsets[(int)CannonIndexToUse] = -(velocity * 0.5f);

            //Dust
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Water, velocity.X, velocity.Y, Scale: 2.0f);
                dust.noGravity = true;
            }
        }
    }
    public class BlueFlybot_Bubble : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 200;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(2);
        }
        public override void AI()
        {
            if (Projectile.timeLeft > 60)
            {
                Projectile.velocity *= 0.99f;
                Projectile.velocity.Y -= 0.02f;
                if (Projectile.velocity.Y < -6)
                    Projectile.velocity.Y = -6;
            }
            else
            {
                Projectile.velocity *= 0.95f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);

            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Water);
            }
        }
    }
}
