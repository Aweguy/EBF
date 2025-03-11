using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFCatToy : ModItem
    {
        private int minionTimer = 0;

        /// <summary>
        /// This property determines which minion appears while the cat toy is held.
        /// <para>Defaults to 0, meaning no minion is spawned.</para>
        /// </summary>
        public int BonusMinion { get; set; } = 0;

        /// <summary>
        /// How long the minion lasts after switching weapons (in ticks).
        /// <para>Defaults to 60 ticks (1 second).</para>
        /// </summary>
        public int MinionDuration { get; set; } = 60;

        public virtual void SetDefaultsSafe()
        { }
        public virtual void HoldItemSafe(Player player)
        { }

        public override void SetDefaults()
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
        public override void HoldItem(Player player)
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
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Top, Vector2.Zero, BonusMinion, Item.damage, Item.knockBack, player.whoAmI);
                }

                //Keep minion alive
                minionTimer = MinionDuration;
            }

            HoldItemSafe(player);
        }
        public override void UpdateInventory(Player player) => CheckAndDespawnMinion(player);
        public override void PostUpdate() => CheckAndDespawnMinion(Main.LocalPlayer);
        
        /// <summary>
        /// Use this method in the shortsword projectile to give the minion some boosted time.
        /// </summary>
        /// <param name="duration">How many ticks the boost will last.</param>
        public void ApplyBoost(int duration)
        {
            Player player = Main.LocalPlayer;
            SoundEngine.PlaySound(SoundID.MaxMana, player.position);

            foreach (int proj in player.ownedProjectileCounts)
            {
                if (Main.projectile[proj].ModProjectile is EBFMinion minion)
                {
                    minion.BoostTime = duration;
                    return;
                }
            }
        }
        private void CheckAndDespawnMinion(Player player)
        {
            // Drain minion time
            if (minionTimer > 0)
            {
                minionTimer--;
            }

            // Despawn the minion when the timer expires
            if (minionTimer <= 0 && BonusMinion > 0 && player.ownedProjectileCounts[BonusMinion] > 0)
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == BonusMinion)
                    {
                        proj.Kill();
                    }
                }
            }
        }
    }
}
