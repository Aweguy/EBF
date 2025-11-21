using EBF.Items.Melee;
using EBF.Items.Placeables.Furniture;
using EBF.Items.Ranged.Guns;
using EBF.Items.Summon;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs
{
    internal class EBFGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            //For anyone editing this, please try to keep it alphabetical
            switch (npc.type)
            {
                //Heavy Claw at 25%
                case NPCID.PrimeVice:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeavyClaw>(), 4));
                    break;

                //Blood Bank at 20%
                case NPCID.ZombieMerman:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodBank>(), 5));
                    break;

                //Godcat Altar at 100%
                case NPCID.MoonLordCore:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GodcatAltar>(), 1));
                    break;
            }
        }
        public override void ModifyShop(NPCShop shop)
        {
            //For anyone editing this, please try to keep it alphabetical
            switch (shop.NpcType)
            {
                case NPCID.ArmsDealer:
                    shop.Add<PowerPaw>(Condition.DownedPlantera);
                    break;

                case NPCID.Dryad:
                    shop.Add<LeafShield>();
                    break;

                case NPCID.Princess:
                    shop.Add<LoveBlade>();
                    break;
            }
        }
    }
}
