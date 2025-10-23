using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;

namespace EBF.NPCs.Idols
{
    public class StoneIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override int HitDustID => DustID.Dirt;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 200;
            NPC.damage = 8;
            NPC.defense = 20;
            NPC.lifeRegen = 4;
            NPC.value = 10;
            goreCount = 4;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
				// Spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
				
                // Description
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.StoneIdol")
            ]);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.StoneBlock, 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneBeach && Main.dayTime && !spawnInfo.Invasion)
                return 0.2f;
            
            return 0f;
        }
    }
}
