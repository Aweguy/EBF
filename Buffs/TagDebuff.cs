using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Buffs
{
    public class TagDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
