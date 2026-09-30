using System.Data;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Idols
{
    public class MetalIdol : IdolNPC
    {
        public override SoundStyle IdolHitSound => SoundID.Item140 with { Pitch = 1.0f, Volume = 1.2f };
        public override int HitDustID => DustID.Iron;
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 50;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeRegen = 4;
            NPC.value = 10;
            goreCount = 4;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
				// Spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				
                // Description
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.MetalIdol")
            ]);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            IItemDropRule eitherorerule = npcLoot.Add(ItemDropRule.Common(ItemID.IronOre, 2, 2, 4));
            eitherorerule.OnFailedRoll(ItemDropRule.Common(ItemID.LeadOre, 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.PlayerSafe && !spawnInfo.Invasion && spawnInfo.Player.ZoneNormalUnderground || !spawnInfo.PlayerSafe && !spawnInfo.Invasion && spawnInfo.Player.ZoneNormalCaverns)
                return 0.02f;

            return 0f;
        }
    }
}
