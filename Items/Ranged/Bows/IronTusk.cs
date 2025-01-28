using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class IronTusk : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 34;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 41;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
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
                type = ModContent.ProjectileType<IronTusk_Arrow>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<Juggernaut>(stack: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 10)
                .AddIngredient(ItemID.Bomb, stack: 20)
                .AddIngredient(ItemID.SoulofMight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class IronTusk_Arrow : EBFChargeableArrow
    {
        public override string Texture => "EBF/Items/Ranged/Bows/Juggernaut_Arrow";
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

            MaximumDrawTime = 50;
            MinimumDrawTime = 10;

            DamageScale = 2f;
            VelocityScale = 2f;
            ReleaseSound = SoundID.Item10;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void PreAISafe()
        {
            if (IsReleased)
            {
                Dust dust;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Scale: 2f);
                dust.noGravity = true;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 2f);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            //Generate explosion
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ProjectileID.Stynger, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
            proj.Kill();
        }
    }
}
