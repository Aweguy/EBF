using EBF.Items.Magic;
using EBF.NPCs.Bosses.Godcat;
using EBF.NPCs.Bosses.NeonValkyrie;
using EBF.NPCs.Machines;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Systems
{
    public enum DifficultyMode : byte { Normal = 0, Expert = 1, Master = 2, Count = 3 }
    public readonly struct DamageValues(int normal, int expert, int master)
    {
        private readonly int[] difficulties = [normal, expert, master];
        public int Get(DifficultyMode idx) => difficulties[(int)idx];
    }

    public static class NPCDamageSystemExtensions
    {
        public static int GetContactDamage(this NPC npc) =>
            ModContent.GetInstance<NPCDamageSystem>().GetContactDamage(npc.type);
        public static int GetProjectileDamage(this NPC npc, int projType) =>
            ModContent.GetInstance<NPCDamageSystem>().GetProjectileDamage(npc.type, projType);
    }


    // Sourced from CalamityMod
    public class NPCDamageSystem : ModSystem
    {
        private Dictionary<int, DamageValues> contactDamage;
        private Dictionary<(int npcType, int projType), DamageValues> projectileDamage;

        public static DifficultyMode CurrentDifficultyMode =>
            Main.masterMode ? DifficultyMode.Master :
            Main.expertMode ? DifficultyMode.Expert :
            DifficultyMode.Normal;

        /// <summary>Gets contact damage based on developer specified values to get around vanilla's poor damage scaling.</summary>
        public int GetContactDamage(int npcType) =>
            contactDamage == null ? 1 : 
            !contactDamage.TryGetValue(npcType, out var damageValues) ? 1 : 
            damageValues.Get(CurrentDifficultyMode);

        /// <summary>Gets projectile damage based on developer specified values to get around vanilla's poor damage scaling.</summary>
        public int GetProjectileDamage(int npcType, int projType) =>
            projectileDamage == null ? 1 :
            !projectileDamage.TryGetValue((npcType, projType), out var damageValues) ? 1 :
            damageValues.Get(CurrentDifficultyMode) / 2; // Divide by 2 to account for hostile projectiles doing double damage

        // Catches mistakes early
        private void ValidateEntries()
        {
            if (contactDamage != null)
            {
                foreach (var kv in contactDamage)
                {
                    for (int i = 0; i < (int)DifficultyMode.Count; i++)
                    {
                        int v = kv.Value.Get((DifficultyMode)i);
                        if (v < 0) throw new Exception($"Contact damage for NPC {kv.Key} has invalid value {v} at index {i}");
                    }
                }
            }

            if (projectileDamage != null)
            {
                foreach (var kv in projectileDamage)
                {
                    for (int i = 0; i < (int)DifficultyMode.Count; i++)
                    {
                        int v = kv.Value.Get((DifficultyMode)i);
                        if (v < 0) throw new Exception($"Projectile damage for ({kv.Key.npcType},{kv.Key.projType}) has invalid value {v} at index {i}");
                    }
                }
            }
        }

        public override void OnModUnload()
        {
            contactDamage = null;
            projectileDamage = null;
        }

        public override void OnModLoad()
        {
            contactDamage = new Dictionary<int, DamageValues>()
            {
                { ModContent.NPCType<Godcat_Light>(), new(1, 1, 1) },
                { ModContent.NPCType<Godcat_Dark>(), new(1, 1, 1) },
                { ModContent.NPCType<Godcat_Creator>(), new(1, 1, 1) },
                { ModContent.NPCType<Godcat_Destroyer>(), new(1, 1, 1) },
                { ModContent.NPCType<BlueCrystal>(), new(80, 70, 70) },
                { ModContent.NPCType<RedCrystal>(), new(80, 70, 70) },

                { ModContent.NPCType<NeonValkyrie>(), new(70, 60, 60) },
                { ModContent.NPCType<RedFlybot>(), new(60, 50, 50) },
                { ModContent.NPCType<BlueFlybot>(), new(60, 50, 50) },
                { ModContent.NPCType<LaserTurret>(), new(70, 60, 60) },
                { ModContent.NPCType<HarpoonTurret>(), new(70, 60, 60) },
                { ModContent.NPCType<CannonTurret>(), new(70, 60, 60) },
                { ModContent.NPCType<NukeStand>(), new(70, 60, 60) },
            };

            projectileDamage = new Dictionary<(int, int), DamageValues>()
            {
                // Godcat
                { (ModContent.NPCType<Godcat_Light>(), ModContent.ProjectileType<Seraphim_Judgement>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Light>(), ModContent.ProjectileType<Godcat_LightBlade>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Light>(), ModContent.ProjectileType<Godcat_LightDiamond>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Dark>(), ModContent.ProjectileType<Godcat_DarkBlade>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Dark>(), ModContent.ProjectileType<Godcat_ReturnBall>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Dark>(), ModContent.ProjectileType<Godcat_BallProjectile>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Creator>(), ModContent.ProjectileType<Godcat_TurningBall>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Creator>(), ModContent.ProjectileType<Godcat_LightDiamond>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Creator>(), ModContent.ProjectileType<Creator_Thunderball>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Creator>(), ModContent.ProjectileType<Creator_HugeThunderball>()), new(140, 120, 120) },
                { (ModContent.NPCType<Godcat_Creator>(), ModContent.ProjectileType<Creator_HolyDeathray>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Destroyer>(), ModContent.ProjectileType<Godcat_TurningBall>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Destroyer>(), ModContent.ProjectileType<Godcat_BallProjectile>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Destroyer>(), ModContent.ProjectileType<Destroyer_DarkHomingBall>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Destroyer>(), ModContent.ProjectileType<Destroyer_DarkBreath>()), new(80, 70, 70) },
                { (ModContent.NPCType<Godcat_Destroyer>(), ModContent.ProjectileType<Destroyer_FireWheel>()), new(80, 70, 70) },
                { (ModContent.NPCType<BlueCrystal>(), ModContent.ProjectileType<ArcticTrident_Icecicle>()), new(160, 140, 140) },
                { (ModContent.NPCType<RedCrystal>(), ProjectileID.DD2PhoenixBowShot), new(80, 70, 70) },

                // Neon Valkyrie
                { (ModContent.NPCType<NeonValkyrie>(), ProjectileID.Bullet), new(70, 60, 60) },
                { (ModContent.NPCType<RedFlybot>(), ModContent.ProjectileType<RedFlybot_Laser>()), new(60, 50, 50) },
                { (ModContent.NPCType<BlueFlybot>(), ModContent.ProjectileType<BlueFlybot_Bubble>()), new(60, 50, 50) },
                { (ModContent.NPCType<LaserTurret>(), ModContent.ProjectileType<LaserTurret_Ball>()), new(60, 50, 50) },
                { (ModContent.NPCType<LaserTurret>(), ModContent.ProjectileType<LaserTurret_Laser>()), new(80, 70, 70) },
                { (ModContent.NPCType<HarpoonTurret>(), ModContent.ProjectileType<HarpoonTurret_Projectile>()), new(120, 100, 100) },
                { (ModContent.NPCType<CannonTurret>(), ProjectileID.CannonballHostile), new(60, 50, 50) },
                { (ModContent.NPCType<NukeStand>(), ModContent.ProjectileType<NuclearBomb>()), new(130, 120, 120) },

            };

            ValidateEntries();
        }
    }
}