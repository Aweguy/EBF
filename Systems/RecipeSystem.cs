using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EBF.Systems
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