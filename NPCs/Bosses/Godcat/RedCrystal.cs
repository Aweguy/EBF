using EBF.Systems;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace EBF.NPCs.Bosses.Godcat
{
    public class RedCrystal : Godcat_CrystalNPC
    {
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld, // Spawn conditions
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.RedCrystal") // Description
            ]);
        }
        protected override void Attack(Player player)
        {
            var type = ProjectileID.DD2PhoenixBowShot;
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, type, NPC.GetProjectileDamage(type), 3f);
            proj.friendly = false;
            proj.hostile = true;
        }
    }
}
