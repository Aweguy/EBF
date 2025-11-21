using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Idols
{
    public class MarbleIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override int HitDustID => DustID.Marble;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 80;
            NPC.damage = 10;
            NPC.defense = 4;
            NPC.lifeRegen = 4;
            NPC.value = 10;
            goreCount = 4;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
				// Spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Marble,
				
                // Description
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.MarbleIdol")
            ]);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.MarbleBlock, 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert || spawnInfo.Player.ZoneUndergroundDesert || spawnInfo.Player.ZoneMarble && !spawnInfo.Invasion)
                return 0.2f;

            return 0f;
        }
    }
}
