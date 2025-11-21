using EBF.Buffs;
using EBF.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFToyStab : ModProjectile
    {
        /// <summary>
        /// How far away from the player the shortsword is held.
        /// </summary>
        protected int ProjOffset { get; set; }

        /// <summary>
        /// How many ticks the bonus minion will be boosted by this cat toy stab projectile. If set to 0, the minion will not be boosted.
        /// <para>Defaults to 180.</para>
        /// </summary>
        protected int BoostDuration { get; set; } = 180;

        /// <summary>
        /// The strength of the tag damage debuff, that should be applied to the hit enemy npc.
        /// <para>Defaults to 2.</para>
        /// </summary>
        protected int TagDamage { get; set; } = 2;
        public virtual void SetDefaultsSafe() { }
        public virtual void OnHitNPCSafe(NPC target, NPC.HitInfo hit, int damageDone) { }
        public sealed override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            SetDefaultsSafe();
        }
        public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Item item = Main.player[Projectile.owner].HeldItem;
            if (item.ModItem is EBFCatToy toy && !target.immortal)
            {
                if (BoostDuration > 0)
                {
                    toy.ApplyBoost(BoostDuration);
                }

                Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
                target.AddBuff(ModContent.BuffType<TagDebuff>(), 300);
                target.GetGlobalNPC<EBFTagDamageGlobalNPC>().dynamicTagDamage = TagDamage;

                //Spawn fancy hit particle
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center });
                SoundEngine.PlaySound(SoundID.MaxMana, Main.LocalPlayer.position);
            }

            OnHitNPCSafe(target, hit, damageDone);
        }
        public sealed override void PostAI()
        {
            Projectile.position += Projectile.velocity * ProjOffset;
        }
    }
}
