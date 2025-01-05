using Terraria.ModLoader;

namespace EBF
{
	public class EBF : Mod
	{
        private static EBF instance;
        public static EBF Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new EBF();
                }
                return instance;
            }
        }
        internal enum EpicMessageType : byte
        {
            EpicPlayerSyncPlayer
        }
    }
}