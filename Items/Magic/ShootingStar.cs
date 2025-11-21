using EBF.EbfUtils;
﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class ShootingStar : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        private const int Spread = 48;
        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 45;//Item's base damage value
            Item.knockBack = 5;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 8;//The amount of mana this item consumes on use

            Item.useTime = 12;//How fast the item is used
            Item.useAnimation = 12;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item43;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
            
            Item.shoot = ModContent.ProjectileType<CrystalStaff_Projectile>();
            Item.shootSpeed = 0.1f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Spawn position
            var offset = new Vector2(16 * player.direction, -256 * player.gravDir);
            position = StaffHead + offset + Main.rand.NextVector2CircularEdge(Spread, Spread);

            //Velocity towards cursor
            velocity = Vector2.Normalize(Main.MouseWorld - position) * velocity.Length();

            //Spawn the projecile
            damage += Main.rand.Next(-20, 21);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<CrystalStaff>(stack: 1)
                .AddIngredient(ItemID.FallenStar, stack: 10)
                .AddIngredient(ItemID.SoulofLight, stack: 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
