﻿using EBF.Abstract_Classes;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class CrimsonDragon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 34;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
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
            Item.shootSpeed = 10f;
            Item.channel = true;
            Item.noMelee = true;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                type = ModContent.ProjectileType<CrimsonDragon_CrimsonArrow>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HellwingBow, stack: 1)
                .AddIngredient(ItemID.HellstoneBar, stack: 10)
                .AddIngredient(ItemID.SoulofFright, stack: 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class CrimsonDragon_CrimsonArrow : EBFChargeableArrow
    {
        private const int batSpawnRate = 3; //How often a projectile is spawned per second

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.hide = false;

            MinimumDrawTime = 10;
            MaximumDrawTime = 40;
            ReleaseSound = SoundID.DD2_FlameburstTowerShot;

            DamageScale = 2.5f;
            VelocityScale = 1.5f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void PreAISafe()
        {
            //Prevent sub-projectiles from being spawned by other players' arrows.
            if (Main.myPlayer != Projectile.owner)
                return;

            if (IsReleased)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, Vector2.Zero, Scale: 0.66f);

                //Run this code x times per second
                if (Main.GameUpdateCount % (60 / batSpawnRate) == 0)
                {
                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity + ProjectileExtensions.GetRandomVector(), ProjectileID.Hellwing, Projectile.damage, Projectile.knockBack);
                    proj.timeLeft = 120;
                }
            }
        }
    }
}
