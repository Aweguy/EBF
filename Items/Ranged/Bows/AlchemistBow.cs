using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class AlchemistBow : EBFBow, ILocalizedModType
    {
        protected override int HoldoutProjectile => ModContent.ProjectileType<AlchemistBow_HoldoutProjectile>();
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 36;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression

            Item.shootSpeed = 10f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.PalladiumBar, stack: 20)
                .AddIngredient(ItemID.HealingPotion, stack: 5)
                .AddIngredient(ItemID.ManaPotion, stack: 5)
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }

    public class AlchemistBow_HoldoutProjectile : EBFHoldoutBow
    {
        public override string Texture => "EBF/Items/Ranged/Bows/AlchemistBow";
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 70;
            
            MaximumDrawTime = 75;
            DamageScale = 1.5f;
            VelocityScale = 2f;
            
            base.SetDefaults();
        }

        protected override void OnShoot(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner, float ai0, float ai1, float ai2)
        {
            if (!FullyCharged)
            {
                base.OnShoot(source, position, velocity, type, damage, knockback, owner);
                return;
            }
            
            var arrows = GetAllArrows();
            for (var i = 0; i < 3; i++)
            {
                // Choose random arrow
                var projectile = arrows[Main.rand.Next(arrows.Count)];
                var proj = Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.15d), projectile, damage, knockback, owner);
                proj.localNPCHitCooldown = -1;
                proj.usesLocalNPCImmunity = true;

                // Hellwing bats are a bit weird, they have to be tweaked
                if (proj.type == ProjectileID.Hellwing)
                {
                    proj.velocity *= 1.5f;
                    proj.timeLeft = 180;
                }
            }
        }

        private static List<int> GetAllArrows()
        {
            List<int> arrows = [];
            
            //Go through every projectile
            for (var i = 0; i < ProjectileID.Count; i++)
            {
                Projectile proj = new();
                proj.SetDefaults(i);

                //Store each vanilla arrow (excluding phantasm arrows cuz their homing has no target)
                if (proj.arrow && proj.ModProjectile == null && proj.type != ProjectileID.PhantasmArrow)
                    arrows.Add(i);
            }

            return arrows;
        }
    }
}
