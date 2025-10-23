using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Buffs.Cooldowns
{
    public class Overheated : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Play audio cue on expiry
            if (player.buffTime[buffIndex] == 2)
                SoundEngine.PlaySound(SoundID.MaxMana);
        }
    }
}
