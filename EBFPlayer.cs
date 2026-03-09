using EBF.Items.Ranged.Bows;
using EBF.Items.Summon;
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
        public override void PostUpdateRunSpeeds()
        {
            // Riot shield slows while held
            if (Player.HeldItem.type == ModContent.ItemType<RiotShield>())
            {
                Player.accRunSpeed *= 0.8f;
                Player.moveSpeed *= 0.8f;
            }

            // Regal turtle slows while shooting
            if (Player.HeldItem.type == ModContent.ItemType<RegalTurtle>() && Player.channel)
            {
                Player.accRunSpeed *= 0.6f;
                Player.moveSpeed *= 0.6f;
            }
        }
        private void ModifyHurtInfo(ref Player.HurtInfo info)
        {
            if (tetrominoEquipped && info.Damage % 10 == 4)
            {
                info.Damage = 4;
            }
        }
    }
}
