using EBF.Items.Materials;
using EBF.NPCs.Bosses.NeonValkyrie;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.BossSummons
{
    public class NeonKeychain : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.BossSummons";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 34;
            Item.maxStack = 99;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.consumable = true;

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 0, platinum: 0);
            Item.rare = ItemRarityID.LightRed;
        }
        public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<NeonValkyrie>());
        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.position);
            int type = ModContent.NPCType<NeonValkyrie>();
            NPC.SpawnOnPlayer(player.whoAmI, type);

            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<AtomicBattery>(stack: 1)
                .AddIngredient<NeonCase>(stack: 1)
                .AddIngredient<MechanicalChain>(stack: 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
