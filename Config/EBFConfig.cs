﻿/*#region Using directives

using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Terraria.ModLoader.Config;

#endregion Using directives

namespace EBF.Config
{
	public class EBFConfig : ModConfig
	{


		// ConfigScope.ClientSide should be used for client side, usually visual or audio tweaks.
		// ConfigScope.ServerSide should be used for basically everything else, including disabling items or changing NPC behaviours
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// The "$" character before a name means it should interpret the name as a translation key and use the loaded translation with the same key.
		// The things in brackets are known as "Attributes".
		//[Header("Flair Slot Vertical Stack")] // Headers are like titles in a config. You only need to declare a header on the item it should appear over, not every item in the category.
		[LabelKey("$FlairSlotVerticalStack")] // A label is the text displayed next to the option. This should usually be a short description of what it does.
		[TooltipKey("$FlairSlotVerticalStack")] // A tooltip is a description showed when you hover your mouse over the option. It can be used as a more in-depth explanation of the option.
		[DefaultValue(false)] // This sets the configs default value.
		public bool FlairVerticalStack; // To see the implementation of this option, see ExampleWings.cs

		//[Header("Flair Slot Position")]
		[LabelKey("$FlairSlotPosition")]
		[TooltipKey("$FlairSlotPosition")]
		[DefaultValue(typeof(Vector2),"900,900")]
		public Vector2 FlairSlotPosition;
	}
}*/