using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static EBF.Items.Magic.FanWeapon;

namespace EBF.Items.Magic
{
    public class PaperFan_White : FanWeapon, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Cobweb, stack: 20)
                .AddIngredient(ItemID.Wood, stack: 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}