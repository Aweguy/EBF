using Terraria;
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
            timer--;
            if (timer <= 0)
            {
                int regen = player.statLifeMax2 / 100 * 5;
                player.Heal(regen);
                timer = 120;
            }
        }
    }
}
