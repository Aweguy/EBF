using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class TestBow : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Phantasm}";

        public override void SetDefaults()
        {
            Item.width = 36;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 10;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item32;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.noUseGraphic = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ModContent.ProjectileType<TestHoldoutBow>();
            Item.shootSpeed = 10f;
            Item.channel = true;
            Item.noMelee = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
	        type = ModContent.ProjectileType<TestHoldoutBow>(); // Item will attempt to shoot ammo item, we must set it back to the held projectile
	        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer);
	        return false;
        }
        
        public override bool CanUseItem(Player player) => player.HasAmmo(player.HeldItem) && !player.noItems && !player.CCed;
        public override bool CanConsumeAmmo(Item ammo, Player player) => !player.ItemTimeIsZero;
    }
    
    public class TestHoldoutBow : EBFHoldoutBow
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Phantasm}";
        protected override int ArrowType => ProjectileID.WoodenArrowFriendly;
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 54;
            base.SetDefaults();
        }
    }
}

