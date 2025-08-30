using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;

namespace EBF.Systems
{
    /// <summary>
    /// Acts as a container for "downed boss" flags.
    /// <br> Set a flag like this in your bosses OnKill hook: NPC.SetEventFlagCleared(ref DownedBossSystem.downedMinionBoss, -1);</br>
    /// <para>Saving and loading these flags requires TagCompounds, a guide exists on the wiki: https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound</para>
    /// </summary>
    public class DownedBossSystem : ModSystem
    {
        public static bool 
            downedNeonValk = false,
            downedGodcat = false;

        public override void ClearWorld()
        {
            downedNeonValk = false;
            downedGodcat = false;
        }

        // We save our data sets using TagCompounds.
        // NOTE: The tag instance provided here is always empty by default.
        public override void SaveWorldData(TagCompound tag)
        {
            if (downedNeonValk) tag["downedNeonValk"] = true;
            if (downedGodcat) tag["downedGodcat"] = true;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedNeonValk = tag.ContainsKey("downedNeonValk");
            downedGodcat = tag.ContainsKey("downedGodcat");
        }

        public override void NetSend(BinaryWriter writer)
        {
            // Order of parameters is important and has to match that of NetReceive
            // WriteFlags supports up to 8 entries, if you have more than 8 flags to sync, call WriteFlags again.
            writer.WriteFlags(downedNeonValk, downedGodcat);
        }

        public override void NetReceive(BinaryReader reader)
        {
            // Order of parameters is important and has to match that of NetSend
            // ReadFlags supports up to 8 entries, if you have more than 8 flags to sync, call ReadFlags again.
            reader.ReadFlags(out downedNeonValk, out downedGodcat);
        }
    }
}
