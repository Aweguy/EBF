using EBF.Buffs.Cooldowns;
using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs
{
	public class BlessedBuff : ModBuff
	{
        public override void SetStaticDefaults()
        {
			base.DisplayName.WithFormatArgs("Blessed");
			base.Description.WithFormatArgs("You have been granted status immunity, Godcat be praised!");
		}

		public override void Update(Player player, ref int buffIndex)
		{
			//player.GetModPlayer<EpicPlayer>().numberOfDrawableBuffs++;

			//player.GetModPlayer<EpicPlayer>().Blessed = true;

			for (int j = 0; j < BuffLoader.BuffCount; ++j)
			{
				if (Main.debuff[j])
				{

					if(j != ModContent.BuffType<CooldownProtect>() && j != ModContent.BuffType<CooldownRegen>())
					{
						player.buffImmune[j] = true;
					}
				}
			}
		}
	}
}