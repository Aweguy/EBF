using EBF.Abstract_Classes;
using EBF.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Ores
{
    public class QuartzOre : OreNPC
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Quartz Ore");
            Main.npcFrameCount[NPC.type] = 6;
        }

        public override void SetSafeDefaults()
        {
            NPC.width = 40;
            NPC.height = 40;

            NPC.lifeMax = 100;
            NPC.damage = 25;
            NPC.defense = 7;
            NPC.lifeRegen = 4;
            NPC.knockBackResist = -0.2f;

            NPC.noGravity = true;

            DrawOffsetY = -5;

            NPC.noTileCollide = true;
            NPC.aiStyle = -1;

            Explosion = ModContent.ProjectileType<QuartzOre_QuartzExplosion>();

            MoveSpeedMultval = 8f;
            MoveSpeedBalval = 100;
            SpeedBalance = 100f;

            DashCooldown = 5;

            DashDistance = 21f;
            DashCharge = 120;
            DashVelocity = 12.5f;
            DashDuration = 180;//Should be higher than the Dash Charge

            StunDuration = 3;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("Ores are part of the eponymous Ore Cycle, delivering magic amassed in the ground into the skies.")
            });
        }

        #region FindFrame

        public override void FindFrame(int frameHeight)
        {
            if (++NPC.frameCounter >= 7)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (frameHeight * Main.npcFrameCount[NPC.type]);
            }
        }

        #endregion FindFrame

        #region CheckDead

        public override bool CheckDead()
        {
            int goreIndex = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction) * -1, Mod.Find<ModGore>("QuartzOre_Gore1").Type, 1f);
            int goreIndex2 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction), Mod.Find<ModGore>("QuartzOre_Gore2").Type, 1f);
            int goreIndex3 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction) * -1, Mod.Find<ModGore>("QuartzOre_Gore3").Type, 1f);
            int goreIndex4 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, (NPC.velocity * NPC.direction), Mod.Find<ModGore>("QuartzOre_Gore4").Type, 1f);

            for (int i = 0; i <= 15; i++)
            {
                Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.Stone, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), Scale: 1);
            }
            for (int j = 0; j <= 5; j++)
            {
                Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.Silver, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), Scale: 1);
            }

            return true;
        }

        #endregion CheckDead

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Diamond));
        }

    }

    public class QuartzOre_QuartzExplosion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 13;
        }

        private int timer2 = 1;
        private int shrink = 0;
        private int baseWidth;
        private int baseHeight;

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.alpha = 1;
            baseWidth = Projectile.width;
            baseHeight = Projectile.height;
            Projectile.scale = 1.5f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            float chance = Main.rand.NextFloat(1f);

            if (chance <= 0.3f)
            {
                for (int i = 0; i < Player.MaxBuffs; ++i)
                {
                    if (target.buffType[i] != 0 && !Main.debuff[target.buffType[i]] && !target.HasBuff(ModContent.BuffType<BlessedBuff>()))
                    {
                        target.DelBuff(i);
                        i--;
                    }
                }
            }
        }

        public override void AI()
        {
            Vector2 oldSize = Projectile.Size;

            timer2--;
            shrink++;
            if (timer2 == 0)
            {
                if (shrink < 5)
                {
                    Projectile.scale += 0.1f;

                    Projectile.width = (int)(baseWidth * Projectile.scale);
                    Projectile.height = (int)(baseHeight * Projectile.scale);
                    Projectile.position = Projectile.position - (Projectile.Size - oldSize) / 2f;

                    timer2 = 1;
                }
                else if (shrink >= 5)
                {
                    Projectile.scale -= 0.05f;

                    Projectile.width = (int)(baseWidth * Projectile.scale);
                    Projectile.height = (int)(baseHeight * Projectile.scale);
                    Projectile.position = Projectile.position - (Projectile.Size - oldSize) / 2f;

                    timer2 = 1;
                }
            }

            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 12)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 64, 64, 64), Color.White, Projectile.rotation, new Vector2(32, 32), Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
