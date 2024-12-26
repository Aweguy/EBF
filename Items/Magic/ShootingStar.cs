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

namespace EBF.Items.Magic
{
    public class ShootingStar : ModItem
    {

        float offsetX = 20f;
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Shooting Star");//Name of the Item
            base.Tooltip.WithFormatArgs("");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 20;//Item's base damage value
            Item.knockBack = 0;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 10;
            Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 20;//How fast the item is used
            Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
            Item.shootSpeed = 5f;
            Item.shoot = ModContent.ProjectileType<Star>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 target = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
            float ceilingLimit = target.Y;
            if (ceilingLimit > player.Center.Y - 200f)
            {
                ceilingLimit = player.Center.Y - 200f;
            }

            position = Main.MouseWorld + new Vector2((-(float)Main.rand.Next(-401, 401) + offsetX) * player.direction, -600f);
            position.Y -= 100;
            Vector2 heading = target - position;
            if (heading.Y < 0f)
            {
                heading.Y *= -1f;
            }
            if (heading.Y < 20f)
            {
                heading.Y = 20f;
            }

            heading.Normalize();
            heading *= new Vector2(velocity.X, velocity.Y).Length();
            velocity.X = heading.X;
            velocity.Y = heading.Y + Main.rand.Next(-40, 41) * 0.02f;
            Projectile.NewProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI, 0f, ceilingLimit);


            return false;
        }
    }

    public class Star : ModProjectile
    {

        int Bounce = 1;
        bool Shrkinking = false;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 10;
            Projectile.knockBack = 1f;
            Projectile.tileCollide = true;
            Projectile.hide = true;
            Projectile.extraUpdates = 2;
            DrawOffsetX = -13;
            DrawOriginOffsetY = -4;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.tileCollide)
            {
                Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);

                if (Bounce > 0)
                {
                    if (Projectile.velocity.X != oldVelocity.X)
                    {
                        Projectile.velocity.X = -oldVelocity.X * 0.3f;
                    }

                    if (Projectile.velocity.Y != oldVelocity.Y)
                    {
                        Projectile.velocity.Y = -oldVelocity.Y * 0.3f;
                        Projectile.velocity.X *= 0.5f;
                    }
                    Bounce--;
                    Shrkinking = true;

                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }



        public override bool PreAI()
        {
            Projectile.velocity.Y += 0.3f; //gravity

            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -16, 16);

            for (int i = 0; i <= 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            }

            if (Shrkinking)
            {
                Projectile.scale -= 0.01f;
                
                if(Projectile.scale <= 0)
                {
                    Projectile.Kill();
                }
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            }
        }

    }
}
