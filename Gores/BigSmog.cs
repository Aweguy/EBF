using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Gores
{
    public class BigSmog : ModGore
    {
        public override void SetStaticDefaults()
        {
            GoreID.Sets.SpecialAI[Type] = 6;
        }
        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.timeLeft = 60;
        }
    }
}
