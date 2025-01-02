using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs.Cooldowns
{
    public class CooldownProtect : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Vulnerable");//Name of the Item
            base.Description.WithFormatArgs("Protection spell is on cooldown.");//Tooltip of the item
            
            Main.debuff[Type] = true;
        }
    }
}
