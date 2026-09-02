using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static EBF.Items.Magic.PaperFan;

namespace EBF.Items.Magic
{
    public class PaperFan_Black : FanWeapon, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<PaperFan>(stack: 1)
                .AddIngredient(ItemID.BlackDye, stack: 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}