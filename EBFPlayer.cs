using Terraria;
using Terraria.ModLoader;

namespace EBF
{
    public class EBFPlayer : ModPlayer
    {
        public bool tetrominoEquipped = false;
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
