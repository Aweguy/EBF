using EBF.Buffs.Cooldowns;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFGun : ModItem
    {
        protected int sidearmType = 0;
        protected int launcherType = 0;
        protected int overheatTime = 60 * 4;

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.autoReuse = true;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.Bullet;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
        }
        public override bool CanUseItem(Player player) => player.altFunctionUse == 2
                ? player.HasAmmo(Item) && !player.HasBuff(ModContent.BuffType<Overheated>())
                : player.HasAmmo(Item);
        public override bool AltFunctionUse(Player player) => true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (sidearmType == 0 || launcherType == 0)
                throw new NullReferenceException("Developer oversight, Sidearm or Launcher type has not been provided.");

            if (player.altFunctionUse == 2)
            {
                player.AddBuff(ModContent.BuffType<Overheated>(), overheatTime);
                type = launcherType;
            }
            else
            {
                type = sidearmType;
            }
        }
    }
}
