using EBF.Abstract_Classes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class TestBow : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<TestHoldoutBow>();
        public override string Texture => $"Terraria/Images/Item_{ItemID.Phantasm}";

        public override void SetDefaults()
        {
            Item.width = 36;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 10;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item32;//The item's sound when it's used
            
            Item.shootSpeed = 16f;
            base.SetDefaults();
        }
    }
    
    public class TestHoldoutBow : EBFHoldoutBow
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Phantasm}";
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 54;
            base.SetDefaults();
        }
    }
}

