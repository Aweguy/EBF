using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EBF
{
    public class EBFPlayer : ModPlayer
    {
        public bool hasHeardDownedNeonValkLine = false; // special line from Lance that we only want to hear once
        public bool tetrominoEquipped = false;
        public override void SaveData(TagCompound tag) => tag["hasHeardDownedNeonValkLine"] = hasHeardDownedNeonValkLine;
        public override void LoadData(TagCompound tag) => hasHeardDownedNeonValkLine = tag.GetBool("hasHeardDownedNeonValkLine");

        public override void ResetEffects() => tetrominoEquipped = false;
        public override void ModifyHurt(ref Player.HurtModifiers modifiers) => modifiers.ModifyHurtInfo += ModifyHurtInfo;
        private void ModifyHurtInfo(ref Player.HurtInfo info)
        {
            if(tetrominoEquipped && info.Damage % 10 == 4)
            {
                info.Damage = 4;
            }
        }
    }
}
