using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class SoulEater : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        //Deals massive damage but reduces your defense by 50% when held.\nHonestly, it could have been worse. It could kill you instantly.
        public override void SetDefaults()
        {
            Item.width = 62;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 62;//Height of the hitbox of the item (usually the item's sprite height)
            Item.scale = 1.4f;//The size multiplier of the item's sprite while it's being used. Also increases range for melee weapons

            Item.damage = 300;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 24;//How fast the item is used
            Item.useAnimation = 24;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 66, silver: 66, gold: 12, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item103;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2)) //Spawning frequency
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.CrimsonTorch);
            }
        }

        public override bool? UseItem(Player player)
        {
            //Drain health every swing
            if (player.itemAnimation == player.itemAnimationMax - 1) //Limit effect to once per attack.
            {
                int hpToDrain = 2;

                //Drain or kill
                if (player.statLife > hpToDrain)
                {
                    player.statLife -= hpToDrain;
                    player.HealEffect(-hpToDrain); //Show the health reduction effect
                }
                else
                {
                    player.KillMe(PlayerDeathReason.ByCustomReason(player.name + " sold their soul."), 10.0, 0);
                } 
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            //50% defense while held
            player.statDefense *= 0.5f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Anarchy>(stack: 1)
                .AddIngredient<BlackFang>(stack: 1)
                .AddIngredient(ItemID.BrokenHeroSword, stack: 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
