using EBF.Items.Ranged.Bows;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFBow : ModItem
    {
        protected abstract int HoldoutProjectile { get; }
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.noUseGraphic = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.UseSound = SoundID.Item32; // Wing flap sound
            Item.channel = true;
            Item.noMelee = true;
            Item.shoot = HoldoutProjectile;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var arrowUsed = type;
            type = HoldoutProjectile;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, arrowUsed);
            return false;
        }

        public override bool CanUseItem(Player player) =>
            player.HasAmmo(player.HeldItem) && !player.noItems && !player.CCed;

        public override bool CanConsumeAmmo(Item ammo, Player player) => !player.ItemTimeIsZero;
    }
}

