using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using EBF.NPCs.Bosses;
using Terraria.Audio;

namespace EBF.Items.Consumeables
{
    public class NeonKeychain : ModItem
    {
        public new string LocalizationCategory => "Items.Consumeables";
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
    }
}
