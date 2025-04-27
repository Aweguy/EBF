using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class SwordBreaker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        private const int debuffDuration = 15; //In seconds
        public override void SetDefaults()
        {
            Item.width = 80;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 80;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 35;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 48;//How fast the item is used
            Item.useAnimation = 48;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 4, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Demonite);
            }
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Weak, 60 * debuffDuration);
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.DemoniteBar, stack: 15)
                .AddIngredient(ItemID.ShadowScale, stack: 10)
                .AddIngredient(ItemID.VilePowder, stack: 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
