using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Idols.IceIdols
{
    public class IceIdol_2 : Idol
    {

        /*public static readonly SoundStyle IdolHit = new("EBF/Assets/Sounds/NPCHit/IceIdolHit")
        {
            Volume = 2f,
            PitchVariance = 1f
        };*/

        public static readonly SoundStyle IdolJump = new("EBF/Assets/Sounds/Custom/Idols/IceIdols/IceIdolJump")
        {
            Volume = 2f,
            PitchVariance = 1f
        };

        public static readonly SoundStyle IdolHighJump = new("EBF/Assets/Sounds/Custom/Idols/IceIdols/IceIdolJump")
        {
            Volume = 2f,
            PitchVariance = 1f
        };

        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Ice Idol");
        }

        public override void SetSafeDefaults()
        {
            NPC.width = 36;
            NPC.height = 48;

            NPC.lifeMax = 75;
            NPC.damage = 16;
            NPC.defense = 5;
            NPC.lifeRegen = 4;
            NPC.value = 50;

            //NPC.HitSound = IdolHit;
            JumpSound = IdolJump;
            HighJumpSound = IdolHighJump;

            PrivateMoveSpeedBalance = 150;
            PrivateMoveSpeedMult = 6f;
            Division = 60f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i <= 5; i++)
            {
                Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.Sand, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), Scale: 1);
            }
        }

        public override bool CheckDead()
        {
            int goreIndex = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction), Mod.Find<ModGore>("StoneIdol2_Gore1").Type, 1f);
            int goreIndex2 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction) * -1, Mod.Find<ModGore>("StoneIdol2_Gore2").Type, 1f);
            int goreIndex3 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction), Mod.Find<ModGore>("StoneIdol2_Gore3").Type, 1f);
            int goreIndex4 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction) * -1, Mod.Find<ModGore>("StoneIdol2_Gore4").Type, 1f);
            int goreIndex5 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction), Mod.Find<ModGore>("StoneIdol2_Gore5").Type, 1f);
            int goreIndex6 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction), Mod.Find<ModGore>("StoneIdol2_Gore6").Type, 1f);

            for (int i = 0; i <= 20; i++)
            {
                Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.Dirt, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), Scale: 1);
            }

            return true;
        }
    }
}
