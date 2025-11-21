using EBF.Items.Magic;
using EBF.Systems;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    public class BlueCrystal : Godcat_CrystalNPC
    {
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow, // Spawn conditions
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.BlueCrystal") // Description
            ]);
        }
        protected override void Attack(Player player)
        {
            var type = ModContent.ProjectileType<ArcticTrident_Icecicle>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, type, NPC.GetProjectileDamage(type), 3f);
            proj.friendly = false;
            proj.hostile = true;
        }
    }
}
