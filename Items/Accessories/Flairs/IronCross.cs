using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
	public class IronCross : ModItem, ILocalizedModType
	{
        public new string LocalizationCategory => "Items.Accessories";
			//base.Tooltip.WithFormatArgs("A war medal which offers protection. Lance's favourite flair.\n4 defense\nBoosts ranged damage by 10%");//Tooltip of the item
		

		public override void SetDefaults()
		{
			Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 32;//Height of the hitbox of the item (usually the item's sprite height)
			Item.defense = 4;
			Item.value = Item.sellPrice(copper: 0, silver: 60, gold: 0, platinum: 0);//Item's value when sold
			Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.accessory = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Ranged) += 0.1f;
		}
        public override void AddRecipes()
        {
			CreateRecipe(amount: 1)
				.AddIngredient(ItemID.IronBar, stack: 15)
				.AddIngredient(ItemID.Bone, stack: 20)
				.AddTile(TileID.Anvils)
				.Register();
        }
    }
}
