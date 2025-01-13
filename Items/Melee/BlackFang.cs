using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
	public class BlackFang : ModItem, ILocalizedModType
	{
        public new string LocalizationCategory => "Items.Weapons.Melee";
		public override void SetDefaults()
		{
			Item.width = 82;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 88;//Height of the hitbox of the item (usually the item's sprite height)
			Item.scale = 0.8f;

			Item.damage = 59;//Item's base damage value
			Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
			Item.useTime = 25;//How fast the item is used
			Item.useAnimation = 25;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper: 0, silver: 70, gold: 6, platinum: 0);//Item's value when sold
			Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression

			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = true;//Boolean, if the player's direction can change while using the item
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Poisoned, 60 * 3);

			if (player.statLife < player.statLifeMax)
			{
				player.Heal(hit.Damage / 20);
			}
		}
        public override void AddRecipes()
        {
			CreateRecipe(amount: 1)
				.AddIngredient<BloodBlade>(stack: 1)
				.AddIngredient(ItemID.SpiderFang, stack: 8)
				.AddIngredient(ItemID.AdamantiteBar, stack: 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
        }
    }
}
