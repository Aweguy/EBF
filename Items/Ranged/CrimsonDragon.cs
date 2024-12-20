using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged
{
    public class CrimsonDragon : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("");//Name of the Item
            base.Tooltip.WithFormatArgs("");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 50;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 20;//How fast the item is used
            Item.useAnimation = 29;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 10f;

            Item.value = Item.sellPrice(copper:, silver:, gold:, platinum:);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.DD2_BallistaTowerShot;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                type = ModContent.ProjectileType<>();
            }
        }
    }

    public class CrimsonDragon_CrimsonArrow : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.tileCollide = true;
            Projectile.hide = true;
            Projectile.extraUpdates = 2;
            DrawOffsetX = -13;
            DrawOriginOffsetY = -4;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
    }
}
