using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class RegalTurtle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 71;//Item's base damage value
            Item.knockBack = 5;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 5, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item32;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
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
                type = ModContent.ProjectileType<RegalTurtle_Arrow>();
            }
        }
        public override void HoldItem(Player player)
        {
            player.statDefense *= 1.5f;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 12)
                .AddIngredient(ItemID.TurtleShell, stack: 2)
                .AddIngredient(ItemID.Ruby, stack: 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class RegalTurtle_Arrow : EBFChargeableArrow
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;

            Projectile.penetrate = 2;

            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.hide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            MaximumDrawTime = 100;
            MinimumDrawTime = 20;
            ReleaseSound = SoundID.Item92;

            DamageScale = 2.25f;
            VelocityScale = 2f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void PreAISafe()
        {
            if (!IsReleased)
            {
                //Reduce player movement speed
                Player player = Main.player[Projectile.owner];
                player.velocity.X = MathHelper.Clamp(player.velocity.X, -4f, 4f);
                player.velocity.Y = MathHelper.Clamp(player.velocity.Y, -6f, 6f);
            }
            else if (Main.rand.NextBool(2))
            {
                //Emit dust on flight
                Dust.NewDust(Projectile.Center, 0, 0, DustID.GoldCoin);
            }
        }
        public override void OnProjectileRelease()
        {
            //Recoil on player
            if (FullyCharged)
            {
                Main.player[Projectile.owner].velocity -= Projectile.velocity / 2;
            }
            else
            {
                Main.player[Projectile.owner].velocity -= Projectile.velocity / 3;
            }
        }
    }
}
