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

namespace EBF.Items.Melee
{
	public class FusionBlade : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Fusion Blade");//Name of the Item
			base.Tooltip.WithFormatArgs("Modeled after the weapons used by the MILITIA branch.\nShoots a big bullet.");//Tooltip of the item
		}

		public override void SetDefaults()
		{
			Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

			Item.damage = 25;//Item's base damage value
			Item.knockBack = 1;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
			Item.useTime = 20;//How fast the item is used
			Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper:10, silver:20, gold:30, platinum:0);//Item's value when sold
			Item.rare = ItemRarityID.Orange;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = false;//Boolean, if the player's direction can change while using the item
			Item.shoot = ModContent.ProjectileType<FusionBlade_BulletBob>();
			Item.shootSpeed = 1f;
		}
    }

	public class FusionBlade_BulletBob : ModProjectile
	{

        private NPC target;
        private float speed = 15;
        private bool runOnce = true;
        private float direction;

        public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;

			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.penetrate = 100;
			Projectile.DamageType = DamageClass.Melee;

			Projectile.timeLeft = 60 * 5;

			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void OnKill(int timeLeft)
		{
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
		}

		public override bool PreAI()
		{
            if (runOnce)
            {
                direction = Projectile.velocity.ToRotation();
                Projectile.rotation = direction + (MathF.PI / 2);
                Projectile.spriteDirection = Projectile.direction;
                runOnce = false;
            }
            Projectile.ai[0]++;
            if (Projectile.ai[0] == 30)
            {
                Projectile.frame = 1;
                Projectile.velocity *= 4.3f;
            }
            else if (Projectile.ai[0] > 30)
            {
             

                if (++Projectile.frameCounter >= 3)
                {

                    direction = Projectile.velocity.ToRotation();
                    Projectile.rotation = direction + (MathF.PI / 2);
					
                    
                    Projectile.frame++;
                    if (Projectile.frame > 2)
                    {
                        Projectile.frame = 1;
                    }
                    //DustAI();
                    Lighting.AddLight(Projectile.Center, new Vector3(255, 165, 0) / 255f);//Orange lighting coming from the center of the Projectile.

                    Player player = Main.player[Projectile.owner];
                    if (Extensions.ProjectileExtensions.ClosestNPC(ref target, 10000, Projectile.Center))
                    {
                        direction = Extensions.ProjectileExtensions.SlowRotation(direction, (target.Center - Projectile.Center).ToRotation(), 3f);
                    }
                    Projectile.velocity = new Vector2(MathF.Cos(direction) * speed, MathF.Sin(direction) * speed);
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Flare);
                    //Projectile.rotation = direction + (MathF.PI / 2);
                }
            }
            else
            {
                Projectile.frame = 0;
            }

			//  float velRotation = Projectile.velocity.ToRotation();
			// Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
			// Projectile.spriteDirection = Projectile.direction;

			return false;
          
        }

		/*public override bool PreAI()
		{

            

            return true;
		}*/

		private void DustAI()
		{
			for (int i = 0; i <= 2; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Firefly);
			}
		}
	}
}
