using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee.Spears
{
    public class GiantSlayer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 74;

            Item.damage = 71;
            Item.knockBack = 6.5f;
            Item.ArmorPenetration = 100;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 9, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;
            
            Item.shoot = ModContent.ProjectileType<GiantSlayer_Projectile>();
            Item.shootSpeed = 7.5f;
            Item.noMelee = true; // Important, the spear is a projectile instead of an item. This prevents the melee hitbox of this item.
            Item.noUseGraphic = true; // Important, it's weird to see two spears at once. This prevents the melee animation of this item.
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 12)
                .AddIngredient(ItemID.SoulofMight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class GiantSlayer_Projectile : ModProjectile
    {
        public float PositionOffset
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;

            Projectile.aiStyle = ProjAIStyleID.Spear;
            Projectile.penetrate = -1;

            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }
        public override void AI()
        {
            DoSpearAI(); // The spear AI is enclosed in a method in case we want to copy that behavior to other items
        }
        private void DoSpearAI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;

            // Set position to the player's center adjusted for mount and step stool
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.position = playerCenter - Projectile.Size / 2;

            // if the player can move, the spear can move
            if (!player.CCed)
            {
                if (PositionOffset == 0f)
                {
                    PositionOffset = 15f; // Set initial offset
                    Projectile.netUpdate = true;
                }

                // Handle move direction based on the item's use animation
                if (player.itemAnimation > player.itemAnimationMax / 2)
                {
                    //Move forward
                    PositionOffset += 0.5f;
                }
                else
                {
                    //Move backward
                    PositionOffset -= 2f;
                }
            }

            // Change the spear position based on the velocity and the offset
            Projectile.position += Projectile.velocity * PositionOffset;

            // Kill projectile if it's done being used
            Projectile.timeLeft = player.itemAnimation;

            // Adjust sprite's rotation to the direction of the stab
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation -= MathHelper.ToRadians(90f);
            }
        }
    }
}
