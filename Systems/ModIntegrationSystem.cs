using System.Collections.Generic;
using System;
using Terraria.ModLoader;
using EBF.Items.Placeables.Furniture.BossRelics;
using EBF.Items.Placeables.Furniture.BossTrophies;
using EBF.NPCs.Bosses.Godcat;
using EBF.Items.BossSummons;
using EBF.Items.Placeables.Furniture;
using EBF.Items.Materials;
using EBF.Items.TreasureBags;
using EBF.NPCs.Bosses.NeonValkyrie;
using Terraria.ID;

namespace EBF.Systems
{
    public class ModIntegrationSystem : ModSystem
    {
        public override void PostSetupContent()
        {
            DoBossChecklistIntegration();
        }

        #region BossChecklist
        private void DoBossChecklistIntegration()
        {
            // Wiki: https://github.com/JavidPack/BossChecklist/wiki/%5B1.4.4%5D-Boss-Log-Entry-Mod-Call
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
                return;

            // Check mod version to ensure the Call messages exist
            if (bossChecklistMod.Version < new Version(1, 6))
                return;

            LogNeonValkyrie(bossChecklistMod);
            LogGodcat(bossChecklistMod);
        }

        private void LogGodcat(Mod bossChecklistMod)
        {
            List<int> npcIDs =
            [
                ModContent.NPCType<Godcat_Light>(),
                ModContent.NPCType<Godcat_Dark>(),
                ModContent.NPCType<Godcat_Creator>(),
                ModContent.NPCType<Godcat_Destroyer>(),
            ];

            List<int> spawnItems =
            [
                ModContent.ItemType<SacredEmerald>(),
                ModContent.ItemType<SacredSapphire>(),
                ModContent.ItemType<SacredRuby>(),
                ModContent.ItemType<GodcatAltar>(),
            ];

            List<int> collectibles =
            [
                ModContent.ItemType<GodcatBossBag>(),
                ModContent.ItemType<GodcatRelic>(),
                ModContent.ItemType<GodcatCreatorTrophy>(),
                ModContent.ItemType<GodcatDestroyerTrophy>(),
                ModContent.ItemType<HolyGrail>(),
                ModContent.ItemType<ElixirOfLife>(),
            ];

            bossChecklistMod.Call(
                "LogBoss", // Method used in the BossChecklist mod
                Mod, // This mod, used as tracking reference by BossChecklist
                "Godcat", //Entry key that can be used by other developers to submit mod-collaborative data to your entry. It should not be changed once defined
                19f, // Weight value inferred from boss progression, see https://github.com/JavidPack/BossChecklist/wiki/Boss-Progression-Values for details
                () => DownedBossSystem.downedGodcat, // Used for tracking checklist progress
                npcIDs,
                new Dictionary<string, object>() // Other optional arguments as needed are inferred from the wiki
                {
                    ["spawnItems"] = spawnItems,
                    ["collectibles"] = collectibles
                }
            );
        }

        private void LogNeonValkyrie(Mod bossChecklistMod)
        {
            List<int> collectibles =
            [
                ModContent.ItemType<NeonValkBossBag>(),
                ModContent.ItemType<NeonValkRelic>(),
                ModContent.ItemType<NeonValkTrophy>(),
                ModContent.ItemType<NanoFibre>(),
                ModContent.ItemType<RamChip>(),
                ItemID.ExplosivePowder,
            ];

            bossChecklistMod.Call(
                "LogBoss", // Method used in the BossChecklist mod
                Mod, // This mod, used as tracking reference by BossChecklist
                "NeonValkyrie", //Entry key that can be used by other developers to submit mod-collaborative data to your entry. It should not be changed once defined
                11.005f, // Weight value inferred from boss progression, see https://github.com/JavidPack/BossChecklist/wiki/Boss-Progression-Values for details
                () => DownedBossSystem.downedNeonValk, // Used for tracking checklist progress
                ModContent.NPCType<NeonValkyrie>(),
                new Dictionary<string, object>() // Other optional arguments as needed are inferred from the wiki
                {
                    ["spawnItems"] = ModContent.ItemType<NeonKeychain>(),
                    ["collectibles"] = collectibles
                }
            );
        }
        #endregion BossChecklist
    }
}
