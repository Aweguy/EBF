using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs
{
    public class Regeneration : ModBuff
    {
        private const int healInterval = 60 * 2;
        public override void Update(Player player, ref int buffIndex)
        {
            if (Main.GameUpdateCount % healInterval == 0 && player.statLife < player.statLifeMax2)
            {
                int regen = player.statLifeMax2 / 100 * 5;
                player.Heal(regen);
            }
        }
    }
}
