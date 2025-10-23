using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;

namespace EBF.NPCs.Idols
{
    public class IceIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override int HitDustID => DustID.Ice;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 95;
            NPC.damage = 17;
            NPC.defense = 3;
            NPC.lifeRegen = 4;
            NPC.value = 10;
            goreCount = 4;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
				// Spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
				
                // Description
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.IceIdol")
            ]);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.IceBlock, 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneSnow && !spawnInfo.Invasion)
                return 0.02f;
            
            return 0f;
        }
    }
}
