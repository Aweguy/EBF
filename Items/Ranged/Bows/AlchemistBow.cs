using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class AlchemistBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 30;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 20;//How fast the item is used
            Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item32;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 10f;
            Item.channel = true;
            Item.noMelee = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.HasAmmo(player.HeldItem) && !player.noItems && !player.CCed;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                type = ModContent.ProjectileType<AlchemistBow_Arrow>();
            }
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

    public class AlchemistBow_Arrow : EBFChargeableArrow
    {
        private List<int> arrows = new List<int>();

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.hide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            MaximumDrawTime = 100;
            MinimumDrawTime = 20;

            DamageScale = 1.66f;
            VelocityScale = 2f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        /*TODO: This probably needs a rework, cuz it takes a while to iterate through every projectile, and this code does it per arrow.
         * It would be better if the bow itself stored all the arrows, so the code only needs to run once. There might also exist a better method to get the arrows that I don't know about.
         * - DigitalZero
         */
        public override void PreAISafe()
        {
            //Run this code once when the bow has fully charged
            if (FullyCharged && Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1]++;

                //Go through every projectile
                for (int i = 0; i < ProjectileID.Count; i++)
                {
                    Projectile proj = new Projectile();
                    proj.SetDefaults(i);

                    //Store each arrow
                    if (proj.arrow && proj.ModProjectile == null)
                    {
                        arrows.Add(i);
                    }
                }
            }
        }
        public override void OnProjectileRelease()
        {
            if (FullyCharged)
            {
                for (int i = 0; i < 3; i++)
                {
                    //Choose random arrow
                    int projectile = arrows[Main.rand.Next(arrows.Count)];
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.2d), projectile, Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }
        }
    }
}
