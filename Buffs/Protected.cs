using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs
{
    public class Protected : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.25f;
        }
    }
}
