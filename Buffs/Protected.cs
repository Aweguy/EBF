using Terraria;
using Terraria.ModLoader;

namespace EBF.Buffs
{
	public class Protected : ModBuff
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Protected");//Name of the Item
			base.Description.WithFormatArgs("Blocks a quarter of damage taken, no Defend needed!");//Tooltip of the item
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.endurance += 0.25f;
		}
	}
}
