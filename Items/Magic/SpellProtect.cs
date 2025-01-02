using EBF.Buffs;
using EBF.Buffs.Cooldowns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class SpellProtect : ModItem
    {
        /*public static readonly SoundStyle ProtectSound = new("EBF/Assets/Sounds/Item/Protect")
        {
            Volume = 1f,
            PitchVariance = 1f
        };*/

        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Protection");//Name of the Item
            base.Tooltip.WithFormatArgs("This spell vastly protects you from enemy attacks.\nBlocks 25% of the damage received.");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 100;
            Item.useAnimation = 10;
            Item.mana = 20;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);
            Item.useTurn = true;
            //Item.UseSound = ProtectSound;
        }
        public override bool CanUseItem(Player player)
        {
            int buff = ModContent.BuffType<CooldownProtect>();
            return !player.HasBuff(buff);
        }
        public override bool? UseItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<Protected>(), 60 * 10);
            player.AddBuff(ModContent.BuffType<CooldownProtect>(), 60 * 40);

            return base.UseItem(player);
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Book, stack: 1)
                .AddIngredient(ItemID.IronskinPotion, stack: 5)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
