using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFCatToy : ModItem
    {
        private int minion = 0;
        private const int minionLingerDuration = 60;

        /// <summary>
        /// This property determines which minion appears while the cat toy is held.
        /// <para>Defaults to 0, meaning no minion is spawned.</para>
        /// </summary>
        public int BonusMinion { get; set; } = 0;

        public virtual void SetDefaultsSafe()
        { }
        public virtual void HoldItemSafe(Player player)
        { }

        public sealed override void SetDefaults()
        {
            Item.DamageType = DamageClass.Summon;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.shoot = ProjectileID.CopperShortswordStab; //Default case
            Item.shootSpeed = 2f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTurn = false;

            SetDefaultsSafe();
        }
        public sealed override bool MeleePrefix() => true;
        public sealed override void HoldItem(Player player)
        {
            if (BonusMinion > 0)
            {
                if (player.ownedProjectileCounts[BonusMinion] < 1)
                {
                    //Kill all other EBF minions
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == player.whoAmI && proj.ModProjectile is EBFMinion)
                        {
                            proj.Kill();
                            break;
                        }
                    }

                    //Spawn new minion
                    minion = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Top, Vector2.Zero, BonusMinion, Item.damage, Item.knockBack, player.whoAmI);
                }

                //Keep minion alive
                Main.projectile[minion].timeLeft = minionLingerDuration;
            }

            HoldItemSafe(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback);
            proj.scale = Item.scale; // Visual only
            proj.velocity *= Item.scale; // Actual gameplay difference
            return false;
        }

        /// <summary>
        /// Use this method in the shortsword projectile to give the minion some boosted time.
        /// </summary>
        /// <param name="duration">How many ticks the boost will last.</param>
        public void ApplyBoost(int duration)
        {
            if (Main.projectile[minion].ModProjectile is EBFMinion m)
            {
                m.BoostTime = duration;
            }
        }
    }
}
