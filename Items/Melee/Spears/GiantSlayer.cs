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
            Item.width = 32;
            Item.height = 32;
            Item.scale = 0.7f;

            Item.damage = 65;
            Item.knockBack = 6.5f;
            Item.ArmorPenetration = 100;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 9, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;
            
            Item.shoot = ModContent.ProjectileType<GiantSlayer_Projectile>();
            Item.shootSpeed = 3.7f;
            Item.noMelee = true; // Important because the spear is actually a projectile instead of an Item. This prevents the melee hitbox of this Item.
            Item.noUseGraphic = true; // Important, it's kind of wired if people see two spears at one time. This prevents the melee animation of this Item.
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
        // This property renames an unclear variable and makes the code more readable
        public float MovementFactor // Change this value to alter how fast the spear moves
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
            DrawOriginOffsetX = -10;
            DrawOriginOffsetY = -10;

            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.defense -= 3;
        }
        public override void AI()
        {
            DoSpearAI(); // The spear AI is enclosed in a method in case we want to copy that behavior to other items
        }
        private void DoSpearAI()
        {
            // Adjust player's held item and itemTime
            Player projOwner = Main.player[Projectile.owner];
            projOwner.heldProj = Projectile.whoAmI;
            projOwner.itemTime = projOwner.itemAnimation;

            // Set position to the player's center adjusted for mount and step stool
            Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            Projectile.position.X = ownerMountedCenter.X - (float)Projectile.width / 2;
            Projectile.position.Y = ownerMountedCenter.Y - (float)Projectile.height / 2;

            // As long as the player isn't frozen, the spear can move
            if (!projOwner.frozen)
            {
                if (MovementFactor == 0f) // When initially thrown out, the ai0 will be 0f
                {
                    MovementFactor = 15f; // Make sure the spear moves forward when initially thrown out
                    Projectile.netUpdate = true; // Make sure to netUpdate this spear
                }

                // Handle move direction based on the item's use animation
                if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2)
                {
                    //Move forward
                    MovementFactor += 0.5f;
                }
                else
                {
                    //Move backward
                    MovementFactor -= 2f;
                }
            }

            // Change the spear position based off of the velocity and the movementFactor
            Projectile.position += Projectile.velocity * MovementFactor;


            // When we reach the end of the animation, we can kill the spear Projectile
            if (projOwner.itemAnimation == 0)
            {
                Projectile.Kill();
            }

            // Adjust sprite's rotation to the direction of the stab
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation -= MathHelper.ToRadians(90f);
            }
        }
    }
}
