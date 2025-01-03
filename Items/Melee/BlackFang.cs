﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
	public class BlackFang : ModItem, ILocalizedModType
	{
        public new string LocalizationCategory => "Items.Weapons.Melee";

			//base.Tooltip.WithFormatArgs("'This weapon is totally historically accurate, I'm sure of it. I saw it in an anime once!' - Matt");//Tooltip of the item
		
		public override void SetDefaults()
		{
			Item.width = 82;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 88;//Height of the hitbox of the item (usually the item's sprite height)

			Item.damage = 62;//Item's base damage value
			Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
			Item.useTime = 22;//How fast the item is used
			Item.useAnimation = 22;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper: 0, silver: 70, gold: 6, platinum: 0);//Item's value when sold
			Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression

			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = true;//Boolean, if the player's direction can change while using the item
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Poisoned, 60 * 5);

			if (player.statLife < player.statLifeMax)
			{
				int HealthHeal = (int)(hit.Damage / 10);
				player.statLife += HealthHeal;
				player.HealEffect(HealthHeal);
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
