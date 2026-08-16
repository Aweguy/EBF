using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Idols
{
    public class WoodenIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override int HitDustID => DustID.t_LivingWood;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 35;
            NPC.damage = 8;
            NPC.defense = 2;
            NPC.lifeRegen = 4;
            NPC.value = 10;
            goreCount = 2;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
				// Spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				
                // Description
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.WoodenIdol")
            ]);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Wood, 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.dayTime && !spawnInfo.PlayerSafe && !spawnInfo.Invasion && spawnInfo.Player.ZoneForest)
                return 0.1f;

            return 0f;
        }
    }
}
