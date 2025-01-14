using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Accessories.Flairs
{
    public class SwordMedal : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 5, platinum: 0);
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.GetDamage(DamageClass.Melee) += 0.15f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 15)
                .AddIngredient(ItemID.SoulofSight, stack: 5)
                .AddIngredient(ItemID.SoulofMight, stack: 5)
                .AddIngredient(ItemID.SoulofFright, stack: 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
