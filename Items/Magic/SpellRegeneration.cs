using EBF.Buffs;
using EBF.Buffs.Cooldowns;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class SpellRegeneration : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.mana = 50;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 0, platinum: 0);
            Item.useTurn = true;
        }
        public override bool CanUseItem(Player player)
        {
            int buff = ModContent.BuffType<CooldownRegen>();
            return !player.HasBuff(buff);
        }
        public override bool? UseItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<Regeneration>(), 60 * 5);
            player.AddBuff(ModContent.BuffType<CooldownRegen>(), 60 * 30);

            return base.UseItem(player);
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Book, stack: 1)
                .AddIngredient(ItemID.RegenerationPotion, stack: 5)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
