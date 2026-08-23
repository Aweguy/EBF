using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class GaiasGift : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<GaiasGift_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 61;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 12, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<GaiasBow>(stack: 1)
                .AddIngredient(ItemID.ChlorophyteBar, stack: 20)
                .AddIngredient(ItemID.LifeFruit, stack: 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class GaiasGift_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<GaiasGift_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/GaiasGift";
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 66;
            MaximumDrawTime = 70;
            DamageScale = 1.33f;
            VelocityScale = 2f;
            base.SetDefaults();
        }
    }

    public class GaiasGift_Arrow : ModProjectile
    {
        private const int gaiaSpawnRate = 12; //How often a projectile is spawned per second
        private bool fullyCharged;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = 5;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            fullyCharged = (int)Projectile.ai[0] == 1;
        }
        public override void AI()
        {
            //Prevent sub-projectiles from being spawned by other players' arrows.
            if (Main.myPlayer != Projectile.owner)
                return;

            //Run this code x times per second
            if (Main.GameUpdateCount % (60 / (gaiaSpawnRate)) == 0 && (fullyCharged))
            {
                var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity / 2 + Main.rand.NextVector2Unit(), ModContent.ProjectileType<GaiaSeed>(), Projectile.damage, Projectile.knockBack);
                proj.timeLeft = 100;
            }
            else if (Main.GameUpdateCount % (60 / gaiaSpawnRate * 2) == 0 && (!fullyCharged))
            {
                var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity / 2 + Main.rand.NextVector2Unit(), ModContent.ProjectileType<GaiaSeed>(), Projectile.damage, Projectile.knockBack);
                proj.timeLeft = 100;
            }
        }
    }
}


