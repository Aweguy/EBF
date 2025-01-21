using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs.Cooldowns
{
    public class Overheated : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}
