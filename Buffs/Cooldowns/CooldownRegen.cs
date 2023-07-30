using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Buffs.Cooldowns
{
    public class CooldownRegen : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Regeneration Sated");
            base.Description.WithFormatArgs("Cannot use Regeneration Spell");
        }
    }
}
