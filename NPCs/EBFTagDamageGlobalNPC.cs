using EBF.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace EBF.NPCs
{
    /// <summary>
    /// This class stores and applies tag damage on every npc. This lets us change the damage value without making new debuff classes per damage value.
    /// </summary>
    public class EBFTagDamageGlobalNPC : GlobalNPC
    {
        public int dynamicTagDamage;
        public override bool InstancePerEntity => true;
        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<TagDebuff>()))
                dynamicTagDamage = 0;
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (dynamicTagDamage > 0 && (projectile.minion || projectile.sentry))
                modifiers.FlatBonusDamage += dynamicTagDamage;
        }
    }
}
