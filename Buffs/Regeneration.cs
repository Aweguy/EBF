using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Buffs
{
    public class Regeneration: ModBuff
    {
        int timer = 0;
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Regeneration");
            base.Description.WithFormatArgs("Quickly regenerating health");
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (--timer <= 0)
            {
                int Regen = player.statLifeMax2 / 100 * 7;
                player.Heal(Regen);
                timer = 60;
            }
        }


    }
}
