using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
	public class IronCross : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Iron Cross");//Name of the Item
			base.Tooltip.WithFormatArgs("A war medal which offers protection. Lance's favourite flair.\n4 defense\nBoosts ranged damage by 10%");//Tooltip of the item
		}

		public override void SetDefaults()
		{
			Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)

			Item.defense = 4;

			Item.value = Item.sellPrice(copper:0, silver:0, gold:2, platinum:0);//Item's value when sold
			Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Generic) += 0.1f;
		}
	}
}
