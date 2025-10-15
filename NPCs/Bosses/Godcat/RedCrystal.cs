using Terraria;
using Terraria.ID;

namespace EBF.NPCs.Bosses.Godcat
{
    public class RedCrystal : Godcat_Crystal
    {
        protected override void Attack(Player player)
        {
            var type = ProjectileID.DD2PhoenixBowShot;
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, type, NPC.damage / 2, 3f);
            proj.friendly = false;
            proj.hostile = true;
        }
    }
}
