using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class BloodBlade : ModItem, ILocalizedModType
    {
        private int regenCooldown;
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 82;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 88;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 16;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 25;//How fast the item is used
            Item.useAnimation = 25;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Blood);
                dust.velocity = Vector2.UnitX * player.direction * 0.33f;
            }
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (player.statLife < player.statLifeMax && regenCooldown <= 0)
            {
                regenCooldown = 60;
                player.Heal(hit.Damage / 4);
            }
        }
        public override void HoldItem(Player player)
        {
            if (regenCooldown > 0)
            {
                regenCooldown--;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.CrimtaneBar, stack: 14)
                .AddIngredient(ItemID.ViciousPowder, stack: 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
