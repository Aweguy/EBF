using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs
{
    public class Regeneration: ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Regeneration");
            base.Description.WithFormatArgs("Quickly regenerating health");
        }
        public override void Update(Player player, ref int buffIndex)
        {
            //Run code once every 120 updates
            if (Main.GameUpdateCount % 120 == 0)
            {
                int regen = player.statLifeMax2 / 100 * 5;
                player.Heal(regen);
            }
        }
    }
}
