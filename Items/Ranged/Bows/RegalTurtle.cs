using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class RegalTurtle : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<RegalTurtle_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 71;//Item's base damage value
            Item.knockBack = 5;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 5, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void HoldItem(Player player)
        {
            player.statDefense *= 1.5f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 12)
                .AddIngredient(ItemID.TurtleShell, stack: 1)
                .AddIngredient(ItemID.Ruby, stack: 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class RegalTurtle_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<RegalTurtle_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/RegalTurtle";
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 66;
            ReleaseSound = SoundID.Item92;
            DamageScale = 2.25f;
            VelocityScale = 2f;
            base.SetDefaults();
        }

        protected override void OnShoot(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner,
            float ai0 = 0, float ai1 = 0, float ai2 = 0)
        {
            var player = Main.player[owner];
            var oldVelocity = player.velocity;

            //Recoil on player
            if (FullyCharged)
                player.velocity -= Projectile.velocity / 2;
            else
                player.velocity -= Projectile.velocity / 3;

            // Prevent infinite horizontal acceleration from recoil
            if (MathF.Abs(player.velocity.X) > 20 && MathF.Abs(player.velocity.X) > MathF.Abs(oldVelocity.X))
                player.velocity.X = oldVelocity.X;
            
            base.OnShoot(source, position, velocity, type, damage, knockback, owner, ai0, ai1, ai2);
        }
    }

    public class RegalTurtle_Arrow : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;

            Projectile.penetrate = 2;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void AI()
        {
            //Emit dust
            if (Main.rand.NextBool(2))
                Dust.NewDust(Projectile.Center, 0, 0, DustID.GoldCoin);
        }
    }
}
