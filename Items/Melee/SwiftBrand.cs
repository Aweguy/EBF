﻿using EBF.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class SwiftBrand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 48;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)
            Item.scale = 1.1f;

            Item.damage = 14;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 18;//How fast the item is used
            Item.useAnimation = 18;//How long the animation lasts. For swords it should stay the same as UseTime
            
            Item.value = Item.sellPrice(copper: 40, silver: 5, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = false;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 position = hitbox.Center.ToVector2() + ProjectileExtensions.GetRandomVector() * 20;
                Vector2 velocity = Vector2.UnitX * player.direction * 0.33f;
                Dust dust = Dust.NewDustPerfect(position, DustID.TintableDustLighted, velocity, 0, Color.WhiteSmoke);
                dust.noLight = true;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.TungstenBar, stack: 12)
                .AddIngredient(ItemID.Feather, stack: 8)
                .AddIngredient(ItemID.Emerald, stack: 4)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
