using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EBF.Dusts
{
    /// <summary>
    /// This dust is used as a hover effect for Neon Valkyrie.
    /// </summary>
    public class HoverRing : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 64, 16);
            dust.alpha = 100;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;
            dust.scale += 0.01f;
            dust.alpha += 10;

            return false;
        }
    }
}