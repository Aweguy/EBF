using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace EBF
{
	public class RecipeSystem : ModSystem
	{
        public override void AddRecipeGroups()
        {
            RecipeGroup anyTitanium = new(() => Language.GetTextValue("LegacyMisc.37") + " Titanium Bar", [ItemID.TitaniumBar, ItemID.AdamantiteBar]);
            RecipeGroup.RegisterGroup("TitaniumBar", anyTitanium);
        }
    }
}