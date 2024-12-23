using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class SoulEater : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Soul Eater");//Name of the Item
            base.Tooltip.WithFormatArgs("Honestly, it could have been worse. It could kill you instantly.\nWhen held, increases your damage by 80% but reduces your defense by 50%.");//Tooltip of the item
        }

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

            Item.value = Item.sellPrice(copper: 66, silver: 66, gold: 6, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
        }

        public override void HoldItem(Player player)//Needs revision
        {
            player.GetDamage(DamageClass.Generic) += 0.8f;
            player.statDefense /= (int)2f;
        }
    }
}
