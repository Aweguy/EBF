using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EBF
{
    public class EBFPlayer : ModPlayer
    {
        public bool tetrominoEquipped = false;

        public override void ResetEffects()
        {
            base.ResetEffects();
            tetrominoEquipped = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            base.ModifyHurt(ref modifiers);
            modifiers.ModifyHurtInfo += ModifyHurtInfo;
        }

        private void ModifyHurtInfo(ref Player.HurtInfo info)
        {
            if(info.Damage % 10 == 4 && tetrominoEquipped)
            {
                //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Damage taken: " + info.Damage), Color.Pink);
                info.Damage = 4;
            }
        }

    }
}
