using Terraria.ModLoader;

namespace EBF
{
	public class EBF : Mod
	{







        public static EBF instance
        {
            get;
            private set;
        }

        public EBF()
        {
            if (EBF.instance == null)
            {
                EBF.instance = this;
            }
        }

        internal enum EpicMessageType : byte
        {
            EpicPlayerSyncPlayer
        }

    }
}