using EBF.Abstract_Classes;
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
    public class CooldownProtect : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Vulnerable");//Name of the Item
            base.Description.WithFormatArgs("Protection spell is on cooldown.");//Tooltip of the item
        }

    }
}
