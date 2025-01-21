using Terraria.ID;
using Terraria.ModLoader;
using EBF.Items.Melee;
using EBF.Items.Ranged.Bows;
using EBF.Items.Ranged.Guns;
using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace EBF.NPCs
{
    internal class EBFGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            //For anyone editing this, please try to keep it alphabetical
            switch (npc.type)
            {
                //Prime Vice:
                //Heavy Claw at 25%
                case NPCID.PrimeVice:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeavyClaw>(), 4));
                    break;
            }
        }
        public override void ModifyShop(NPCShop shop)
        {
            //For anyone editing this, please try to keep it alphabetical
            switch (shop.NpcType)
            {
                case NPCID.Princess:
                    shop.Add<LoveBlade>();
                    break;

                case NPCID.WitchDoctor:
                    shop.Add<GaiasBow>();
                    break;
            }
        }
    }
}
