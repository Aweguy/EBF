using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee.Spears
{
    public class GiantSlayer : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Giant Slayer");//Name of the Item
        }

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 50;
            Item.useTime = 50;
            Item.shootSpeed = 3.7f;
            Item.knockBack = 6.5f;
            Item.width = 32;
            Item.height = 32;
            Item.scale = 0.7f;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.DamageType = DamageClass.Melee; ;
            Item.noMelee = true; // Important because the spear is actually a projectile instead of an Item. This prevents the melee hitbox of this Item.
            Item.noUseGraphic = true; // Important, it's kind of wired if people see two spears at one time. This prevents the melee animation of this Item.
            Item.shoot = ModContent.ProjectileType<GiantSlayer_Projectile>();
        }


        public override void HoldItem(Player player)
        {
            player.GetArmorPenetration(DamageClass.Melee) += 100;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;
            if (Collision.CanHit(position, 100000000, 100000000, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            return true;
        }
    }

    public class GiantSlayer_Projectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 19;
            Projectile.penetrate = -1;
            //Projectile.scale = 1.3f;
            Projectile.alpha = 0;
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

        // In here the AI uses this example, to make the code more organized and readable
        // Also showcased in ExampleJavelinProjectile.cs
        public float movementFactor // Change this value to alter how fast the spear moves
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // It appears that for this AI, only the ai0 field is used!
        public override void AI()
        {
            // Since we access the owner player instance so much, it's useful to create a helper local variable for this
            // Sadly, Projectile/ModProjectile does not have its own
            Player projOwner = Main.player[Projectile.owner];
            // Here we set some of the Projectile's owner properties, such as held item and itemtime, along with Projectile direction and position based on the player
            Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            Projectile.direction = projOwner.direction;
            projOwner.heldProj = Projectile.whoAmI;
            projOwner.itemTime = projOwner.itemAnimation;
            Projectile.position.X = ownerMountedCenter.X - (float)(Projectile.width / 2);
            Projectile.position.Y = ownerMountedCenter.Y - (float)(Projectile.height / 2);
            // As long as the player isn't frozen, the spear can move
            if (!projOwner.frozen)
            {
                if (movementFactor == 0f) // When initially thrown out, the ai0 will be 0f
                {
                    movementFactor = 15f; // Make sure the spear moves forward when initially thrown out
                    Projectile.netUpdate = true; // Make sure to netUpdate this spear
                }
                if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2) // Somewhere along the item animation, make sure the spear moves back
                {
                    movementFactor -= 2f;
                }
                else // Otherwise, increase the movement factor
                {
                   
                    movementFactor += 0.5f;
                }
                
            }
            // Change the spear position based off of the velocity and the movementFactor
            Projectile.position += Projectile.velocity * movementFactor;
            // When we reach the end of the animation, we can kill the spear Projectile
            if (projOwner.itemAnimation == 0)
            {
                Projectile.Kill();
            }
            // Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
            // MathHelper.ToRadians(xx degrees here)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            // Offset by 90 degrees here
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation -= MathHelper.ToRadians(90f);
            }
        }
    }
}
