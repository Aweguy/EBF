﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
	public class Inferno : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Inferno");//Name of the Item
			base.Tooltip.WithFormatArgs("Wreathed in scorching flames.\nBurns foes.");//Tooltip of the item
		}

		public override void SetDefaults()
		{
			Item.width = 60;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 60;//Height of the hitbox of the item (usually the item's sprite height)

			Item.damage = 20;//Item's base damage value
			Item.knockBack = 1f;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
			Item.useTime = 20;//How fast the item is used
			Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper:0, silver:50, gold:1, platinum:0);//Item's value when sold
			Item.rare = ItemRarityID.Cyan;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = true;//Boolean, if the player's direction can change while using the item
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 60 * 2);
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.NextBool(3))
			{
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Firefly);
			}
		}
	}
}
