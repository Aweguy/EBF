using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;

namespace EBF.Extensions
{
    public static partial class Utils
    {
        public enum ExplosionSize { Small, Medium, Large }

        /// <summary>
        /// Creates a bunch of dust and gore to simulate an explosion effect.
        /// <br>Has an overload that supports various sizes when supplied with an ExplosionSize enum.</br>
        /// <para>NOTE: This does not create a hitbox.</para>
        /// </summary>
        /// <param name="entity">The entity that explodes. Used to determine position and area of where the particles spawn.</param>
        public static void CreateExplosionEffect(this Entity entity) => CreateExplosionEffect(entity, ExplosionSize.Medium);

        /// <summary>
        /// Creates a bunch of dust and gore to simulate an explosion effect.
        /// <para>NOTE: This does not create a hitbox.</para>
        /// </summary>
        /// <param name="entity">The entity that explodes. Used to determine position and area of where the particles spawn.</param>
        /// <param name="intensity">How big the explosion should be.</param>
        public static void CreateExplosionEffect(this Entity entity, ExplosionSize intensity)
        {
            // Handle intensity
            (float scale, int fireCount, int smokeCount, int bigSmokeCount, int speed) stats;
            stats = intensity switch
            {
                ExplosionSize.Small => (1, 12, 6, 2, 4),
                ExplosionSize.Medium => (2, 20, 8, 3, 6),
                ExplosionSize.Large => (3, 50, 20, 4, 8),
                _ => throw new System.NotImplementedException(),
            };

            Dust dust;

            // Smoke Dust spawn
            for (int i = 0; i < stats.smokeCount; i++)
            {
                dust = Dust.NewDustDirect(entity.position, entity.width, entity.height, DustID.Smoke, Alpha: 100, Scale: stats.scale);
                dust.velocity += Vector2.Normalize(dust.position - entity.Center) * stats.speed;
            }
            // Fire Dust spawn
            for (int i = 0; i < stats.fireCount; i++)
            {
                var scale = Main.rand.NextFloat(stats.scale / 2, stats.scale);
                dust = Dust.NewDustDirect(entity.position, entity.width, entity.height, DustID.Torch, Alpha: 100, newColor: Color.Yellow, Scale: scale);
                dust.velocity += Vector2.Normalize(dust.position - entity.Center) * 2;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < stats.bigSmokeCount; g++)
            {
                Gore.NewGoreDirect(entity.GetSource_Death(), entity.Center, VectorUtils.Random(1.5f), Main.rand.Next(61, 64), Scale: stats.scale / 2);
            }
        }
    }
}
