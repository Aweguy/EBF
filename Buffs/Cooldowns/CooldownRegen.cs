using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs.Cooldowns
{
    public class CooldownRegen : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}
