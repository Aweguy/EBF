﻿using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace EBF.Items.Melee
{
    public class Avenger : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 52;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 52;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 23;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 26;//How fast the item is used
            Item.useAnimation = 26;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 30, silver: 85, gold: 2, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression

            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (player.statLife < player.statLifeMax)
            {
                int missingHP = player.statLifeMax - player.statLife;
                damage += missingHP * 0.005f; //+20 at most
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MeteoriteBar, stack: 18)
                .AddIngredient(ItemID.Diamond, stack: 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
