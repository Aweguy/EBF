using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;

namespace EBF.NPCs.Idols
{
    public class ObsidianIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override SoundStyle IdolJumpSound => SoundID.Item140 with { Pitch = 1.05f, Volume = 0.3f };
        public override SoundStyle IdolBigJumpSound => SoundID.Item140 with { Pitch = 1.1f, Volume = 0.5f };
        public override int HitDustID => DustID.Obsidian;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 125;
            NPC.damage = 8;
            NPC.defense = 7;
            NPC.lifeRegen = 4;
            NPC.value = 10;
            goreCount = 4;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
				// Spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
				
                // Description
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.ObsidianIdol")
            ]);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Obsidian, 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneUnderworldHeight && !spawnInfo.Invasion)
                return 0.08f;
            
            return 0f;
        }
    }
}
