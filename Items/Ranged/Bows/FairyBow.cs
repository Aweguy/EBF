﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class FairyBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 30;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 40;//Item's base damage value
            Item.knockBack = 2.5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 20;//How fast the item is used
            Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item5;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
            Item.channel = true;
            Item.noMelee = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.HasAmmo(player.HeldItem) && !player.noItems && !player.CCed;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                type = ModContent.ProjectileType<FairyBow_Arrow>();
            }
        }
    }

    public class FairyBow_Arrow : EBFChargeableArrow
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.hide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            MaximumDrawTime = 50;
            MinimumDrawTime = 20;
            AutoRelease = true;

            DamageScale = 1.5f;
            VelocityScale = 1.33f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnProjectileRelease()
        {
            if (FullyCharged)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.33d), ProjectileID.Leaf, Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }
        }
    }
}
