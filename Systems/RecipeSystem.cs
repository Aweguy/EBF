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
            RecipeGroup.RegisterGroup("EvilPowder", new(() => Language.GetTextValue("LegacyMisc.37") + " Vile Powder", [ItemID.VilePowder, ItemID.ViciousPowder]));
            RecipeGroup.RegisterGroup("MythrilBar", new(() => Language.GetTextValue("LegacyMisc.37") + " Mythril Bar", [ItemID.MythrilBar, ItemID.OrichalcumBar]));
            RecipeGroup.RegisterGroup("TitaniumBar", new(() => Language.GetTextValue("LegacyMisc.37") + " Titanium Bar", [ItemID.TitaniumBar, ItemID.AdamantiteBar]));
        }
    }
}