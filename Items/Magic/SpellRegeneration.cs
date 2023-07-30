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
            Item.mana = 100;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(silver: 50);
            Item.useTurn = true;
            Item.value = Item.sellPrice(gold: 10);
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
    }
}
