/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace EBF.Items.Melee
{
    public class DeathScythe : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Death Scythe");//Name of the Item
            base.Tooltip.WithFormatArgs("");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 20;//Item's base damage value
            Item.knockBack = 2f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 25;//How fast the item is used
            Item.useAnimation = 25;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 9, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.NewProjectile(EntitySource_OnHit(), target.Center - new Vector2(15, 0), 0,);
        }
    }

    public class Death : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.tileCollide = true;
            Projectile.hide = true;
            Projectile.extraUpdates = 2;
            DrawOffsetX = -13;
            DrawOriginOffsetY = -4;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }



    }
}
*/