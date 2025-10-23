using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;

namespace EBF.NPCs.Idols
{
    public class WoodenIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override int HitDustID => DustID.t_LivingWood;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 95;
            NPC.damage = 16;
            NPC.defense = 5;
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
            Player player = Main.player[spawnInfo.Player.whoAmI];
            if (PlayerIsInForest(player) && Main.dayTime && !spawnInfo.Invasion)
                return 0.1f;
            
            return 0f;
        }

        private static bool PlayerIsInForest(Player player)
        {
            return !player.ZoneJungle
                && !player.ZoneDungeon
                && !player.ZoneCorrupt
                && !player.ZoneCrimson
                && !player.ZoneHallow
                && !player.ZoneSnow
                && !player.ZoneDesert
                && !player.ZoneUndergroundDesert
                && !player.ZoneGlowshroom
                && !player.ZoneMeteor
                && !player.ZoneBeach
                && player.ZoneOverworldHeight;
        }
    }
}
