using EBF.Buffs;
using EBF.Buffs.Cooldowns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class SpellRegeneration : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Regeneration Spell");
            base.Tooltip.WithFormatArgs("This spell vastly increases your regeneration.\nCosts a lot of mana and has a big cooldown.");
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 100;
            Item.useAnimation = 10;
            Item.mana = 50;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);
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
