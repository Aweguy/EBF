using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.BossSummons
{
    public class SacredSapphire : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 0, platinum: 1);
            Item.rare = ItemRarityID.Master;
        }
    }

    public class SacredEmerald : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 32;
            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 0, platinum: 1);
            Item.rare = ItemRarityID.Master;
        }
    }

    public class SacredRuby : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 38;
            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 0, platinum: 1);
            Item.rare = ItemRarityID.Master;
        }
    }
}
