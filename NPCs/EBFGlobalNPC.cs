using Terraria.ID;
using Terraria.ModLoader;
using EBF.Items.Melee;
using EBF.Items.Ranged.Bows;

namespace EBF.NPCs
{
    internal class EBFGlobalNPC : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            int type = shop.NpcType;

            if (type == NPCID.Princess)
            {
                shop.Add<LoveBlade>();
            }
            if(type == NPCID.WitchDoctor)
            {
                shop.Add<GaiasBow>();
            }
        }
    }
}
