using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
	public class BloodBlade : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Blood Blade");//Name of the Item
			base.Tooltip.WithFormatArgs("Drains the health of hit targets");//Tooltip of the item
		}

		public override void SetDefaults()
		{
			Item.width = 82;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 88;//Height of the hitbox of the item (usually the item's sprite height)

			Item.damage = 14;//Item's base damage value
			Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
			Item.useTime = 28;//How fast the item is used
			Item.useAnimation = 28;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 1, platinum: 0);//Item's value when sold
			Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = true;//Boolean, if the player's direction can change while using the item
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (player.statLife < player.statLifeMax)
			{
				int HealthHeal = (int)(hit.Damage / 5);
				player.statLife += HealthHeal;
				player.HealEffect(HealthHeal);
			}
		}

		/*TODO: Remove recipe and add it to the Matt NPC's shop
		 */
        public override void AddRecipes()
        {
			CreateRecipe(amount: 1)
				.AddIngredient(ItemID.CrimtaneBar, stack: 14)
				.AddIngredient(ItemID.ViciousPowder, stack: 15)
				.AddTile(TileID.Anvils)
				.Register();
        }
    }
}
