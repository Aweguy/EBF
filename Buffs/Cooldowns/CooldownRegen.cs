using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs.Cooldowns
{
    public class CooldownRegen : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Regeneration Sated");
            base.Description.WithFormatArgs("Cannot use Regeneration Spell");

            Main.debuff[Type] = true;
        }
    }
}
