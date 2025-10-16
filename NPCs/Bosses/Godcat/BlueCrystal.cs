using EBF.Items.Magic;
using Terraria;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    public class BlueCrystal : Godcat_Crystal
    {
        protected override void Attack(Player player)
        {
            var type = ModContent.ProjectileType<ArcticTrident_Icecicle>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, type, NPC.damage / 2, 3f);
            proj.friendly = false;
            proj.hostile = true;
        }
    }
}
