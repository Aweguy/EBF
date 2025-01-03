using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class UltraPro9000X : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        //'I can use this old hockey stick as a weapon. I think that should be enough equipment for now.' - Matt

        public override void SetDefaults()
        {
            Item.width = 116;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 100;//Height of the hitbox of the item (usually the item's sprite height)
            Item.scale = 0.7f;//The size multiplier for the item's sprite and hitbox range

            Item.damage = 12;//Item's base damage value
            Item.knockBack = 13f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 41;//How fast the item is used
            Item.useAnimation = 41;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 85, silver: 20, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
        }

        /* TODO: Find the right values for this so it doesn't look stupid
         */
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset;

            if (player.itemRotation < 0f)
            {
                // Swinging upwards
                offset = new Vector2(player.direction * 10f, player.gravDir * 5f);
            }
            else 
            {
                //Swinging downwards
                offset = new Vector2(player.direction * 5f, player.gravDir * 5f);
            }

            offset = offset.RotatedBy(player.itemRotation);
            player.itemLocation += offset;
        }

        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Wood, stack: 40)
                .AddIngredient(ItemID.Silk, stack: 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
