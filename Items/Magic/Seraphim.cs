using EBF.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
	public class Seraphim : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Seraphim");//Name of the Item
			base.Tooltip.WithFormatArgs("A glorious staff used by gorgeous angels.");//Tooltip of the item
		}

		public override void SetDefaults()
		{
			Item.width = 90;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 90;//Height of the hitbox of the item (usually the item's sprite height)

			Item.damage = 20;//Item's base damage value
			Item.knockBack = 1f;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
			Item.useTime = 10;//How fast the item is used
			Item.useAnimation = 10;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper:0, silver:0, gold: 0, platinum: 0);//Item's value when sold
			Item.rare = ItemRarityID.Purple;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = false;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = true;//Boolean, if the player's direction can change while using the item

			Item.shoot = ModContent.ProjectileType<Seraphim_Judgement>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int posX = (int)((float)Main.mouseX + Main.screenPosition.X) / 16;
			int posY = (int)((float)Main.mouseY + Main.screenPosition.Y) / 16;
			if (player.gravDir == -1f)
			{
				posY = (int)(Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY) / 16;
			}
			for (; posY < Main.maxTilesY 
				&& Main.tile[posX, posY] != null && !WorldGen.SolidTile2(posX, posY) 
				&& Main.tile[posX - 1, posY] != null && !WorldGen.SolidTile2(posX - 1, posY) 
				&& Main.tile[posX + 1, posY] != null && !WorldGen.SolidTile2(posX + 1, posY); posY++) { }

			Projectile.NewProjectile(source, (float)Main.mouseX + Main.screenPosition.X, (float)(posY * 16), 0f, 0f, type, damage, 0f, player.whoAmI, 0f, 0f);

			return true;
		}
		public override bool CanUseItem(Player player)
		{
			//Can't use it if your judgement still exists
			return player.ownedProjectileCounts[ModContent.ProjectileType<Seraphim_Judgement>()] < 1;
		}
	}

	public class Seraphim_Judgement : ModProjectile
	{
		public static readonly SoundStyle JudgementSound = new("EBF/Assets/Sounds/Custom/Judgement")
		{
			Volume = 2f,
			PitchVariance = 1f
		};

		#region Variables and Constants

		private const float MAX_CHARGE = 40f;

		//The distance charge particle from the player center
		public float MOVE_DISTANCE = 20f;

		// The actual distance is stored in the ai0 field
		// By making a property to handle this it makes our life easier, and the accessibility more readable
		public float Distance;

		//{
		//get => Projectile.ai[0];
		//set => Projectile.ai[0] = value;
		//}

		// The actual charge value is stored in the localAI0 field
		public float Charge
		{
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		public bool IsAtMaxCharge => Charge == MAX_CHARGE;

		private float scaled = 5f;//Used for the animation illusion
		private float increaseY = 0f; //It increases the Y axis of the dust spawning
		private float increaseY2 = 0f;//Same as above
		private float WaveFrequency = 70f;//Dust spawning wave frequency on both spawners
		private float WaveLength = 100f;//Dust Spawning wave length on both spawners
		private float beamWidth = 100f;//Collision hitbox.
		private float offDistance = 0.6f;//Distance Reduction

		private Vector2 position;//the initial position of the laser
		private Vector2 spriterotation = new Vector2(0, -1);//rotation of the laser to look up

		private int timer0 = 0;//Dust spawning timer for the feathers
		private int timer1 = 0;//Dust spawning for the bubbles
		private int animation = 0;//Sets 0 or 1 for a small animation.

		#endregion Variables and Constants

		public override void SetDefaults()
		{
			Projectile.width = 0;
			Projectile.height = 0;

			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;

			Projectile.timeLeft = 120 + (int)MAX_CHARGE;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.hide = true;

			Projectile.localNPCHitCooldown = 7;
			Projectile.usesLocalNPCImmunity = true;

		}
		public override bool PreDraw(ref Color lightColor)
		{
			if (!IsAtMaxCharge)//When it's not at max charge have a small laser beam
			{
				scaled = 1f;
				MOVE_DISTANCE = 4f;
			}
			else if (IsAtMaxCharge && Projectile.timeLeft <= 80)//if it's at max charge and some time has passed reduce its scale.
			{
				scaled -= 0.06f;
				MOVE_DISTANCE -= 0.24f;
			}
			else//The animation while damaging
			{
				if (animation == 0)
				{
					scaled = 5.5f;
					animation = 1;
					MOVE_DISTANCE = 20f;
				}
				else if (animation == 1)
				{
					scaled = 5f;
					animation = 0;
					MOVE_DISTANCE = 20f;
				}
			}
			if (scaled <= 0f)
			{
				Projectile.Kill();
			}

			DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, position, spriterotation, 10, Projectile.damage, -1.57f, 1f * scaled, 1000f, Color.White, (int)MOVE_DISTANCE);
			return false;
		}
		public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 0)
		{
			float rot = unit.ToRotation() + rotation;

			var origin = Vector2.Zero;

			// Draws the laser 'body'
			for (float i = transDist; i <= Distance; i += step)
			{
				Color c = Color.White;
				origin = start + i * unit;

				spriteBatch.Draw(texture, origin - Main.screenPosition, new Rectangle(0, 26, 28, 26), i < transDist ? Color.Transparent : c, rot, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);
			}

			// Draws the laser 'tail'
			spriteBatch.Draw(texture, start + unit * (transDist - step) - Main.screenPosition,
				new Rectangle(0, 0, 28, 26), Color.White, rot, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);

			// Draws the laser 'head'
			/*spriteBatch.Draw(texture, start + (Distance + step) * unit - Main.screenPosition,
				new Rectangle(0, 52, 28, 26), Color.White, r, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);*/
		}

		// Change the way of collision check of the Projectile
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!IsAtMaxCharge)
			{
				return false;
			}
			// We can only collide if we are at max charge, which is when the laser is actually fired
			Vector2 unit = spriterotation;
			float point = 0f;

			// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
			// It will look for collisions on the given line using AABB
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, position + unit * Distance, beamWidth, ref point);
		}

        /*public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<LightExplosion>(), Projectile.damage, 0, Projectile.owner);
		}*/

        public override void OnSpawn(IEntitySource source)
        {
			//Ground detection
            Player player = Main.player[Projectile.owner];

            int posX = (int)((float)Main.mouseX + Main.screenPosition.X) / 16;
            int posY = (int)((float)Main.mouseY + Main.screenPosition.Y) / 16;
            if (player.gravDir == -1f)
            {
                posY = (int)(Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY) / 16;
            }

            //Find floor
            for (; posY < Main.maxTilesY
                && Main.tile[posX, posY] != null && !WorldGen.SolidTile2(posX, posY)
                && Main.tile[posX - 1, posY] != null && !WorldGen.SolidTile2(posX - 1, posY)
                && Main.tile[posX + 1, posY] != null && !WorldGen.SolidTile2(posX + 1, posY); posY++) { }

            Projectile.position = new Vector2((float)Main.mouseX + Main.screenPosition.X, (float)(posY * 16));
            position = Projectile.position;
        }

        public override void AI()
		{

			Player player = Main.player[Projectile.owner];
			

			#region Beam Shrinking

			if (IsAtMaxCharge && Projectile.timeLeft <= 80)
			{
				beamWidth -= 0.5f;//reducing the hitbox.
			}
			else
			{
				beamWidth = 100f;
			}

			#endregion Beam Shrinking

			// By separating large AI into methods it becomes very easy to see the flow of the AI in a broader sense
			// First we update player variables that are needed to channel the laser
			// Then we run our charging laser logic
			// If we are fully charged, we proceed to update the laser's position
			// Finally we spawn some effects like dusts and light

			UpdatePlayer(player);
			ChargeLaser(player);

			SetLaserPosition();
			SpawnDusts(player);
			CastLights();
		}

		private void SpawnDusts(Player player)
		{
			Vector2 unit = position * -1;

			#region generalDust


			for (int i = 0; i < 1; ++i)
			{
				Vector2 dustVel = new Vector2(1, 0).RotatedBy(Main.rand.NextFloat(1.57f, 1.57f) + (Main.rand.Next(2) == 0 ? -1.0f : 1.0f) * 1.57f);

				Dust dust = Main.dust[Dust.NewDust(new Vector2(position.X, position.Y), 0, 0, DustID.Electric, dustVel.X * 10, dustVel.Y * 10, 0, Color.White)];
				dust.noGravity = true;
				dust.scale = 1.2f;
				dust.shader = GameShaders.Armor.GetSecondaryShader(64, Main.LocalPlayer);
				dust = Dust.NewDustDirect(new Vector2(position.X, position.Y), 0, 0, DustID.Smoke, -unit.X * Distance, -unit.Y * Distance);
				dust.fadeIn = 0f;
				dust.noGravity = true;
				dust.scale = 0.88f;
				dust.color = Color.White;
				dust.shader = GameShaders.Armor.GetSecondaryShader(64, Main.LocalPlayer);
			}

			#endregion generalDust

			#region Feathers

				timer0++;

			if (timer0 == 40)
			{
				if (!Main.dedServ)
					SoundEngine.PlaySound(JudgementSound, Projectile.Center);

				for (int i = 0; i < 85; ++i)
				{
					Vector2 dustVel = new Vector2(1, 0).RotatedBy(Main.rand.NextFloat(1.57f, 2.57f) + (Main.rand.Next(2) == 0 ? -1.6f : 1.0f) * 1.57f);

					float rand = Main.rand.NextFloat(5f, 20f);

					Dust dust = Main.dust[Dust.NewDust(new Vector2(position.X, position.Y), 0, 0, ModContent.DustType<LightFeather>(), dustVel.X * rand, dustVel.Y * rand)];
					dust.noGravity = true;
					dust.noLight = true;
					dust.scale = 1.2f;
					dust.alpha += 2;
					dust = Dust.NewDustDirect(new Vector2(position.X, position.Y), 0, 0, DustID.Smoke,
						-unit.X * Distance, -unit.Y * Distance);
					dust.fadeIn = 0f;
					dust.noGravity = true;
					dust.scale = 0.88f;
					dust.alpha += 2;
					dust.color = Color.Cyan;
				}
			}

			#endregion Feathers

			#region SpiralDust
			
			timer1++;

			if (timer1 >= 40)
			{
				if (Main.GameUpdateCount % 2 == 0)
				{
					Dust dust = Dust.NewDustPerfect(new Vector2((float)(position.X + (WaveLength * Math.Sin(increaseY / WaveFrequency))), position.Y - increaseY), ModContent.DustType<LightBubble>(), new Vector2(0, 0));
					dust.noGravity = true;
					dust.scale = 1.2f;
					dust = Dust.NewDustDirect(new Vector2(position.X, position.Y), 0, 0, DustID.Smoke,
						-unit.X * Distance, -unit.Y * Distance);
					dust.fadeIn = 0f;
					dust.noGravity = true;
					dust.scale = 0.88f;
					dust.color = Color.Cyan;
				}

				if (increaseY < Distance)//Moving the spiral up
				{
					increaseY += 10;
				}
			}

			if (timer1 >= 40)
			{
				if (Main.GameUpdateCount % 2 == 0)
				{
					Dust dust2 = Dust.NewDustPerfect(new Vector2((float)(position.X - (WaveLength * Math.Sin(increaseY2 / WaveFrequency))), position.Y - increaseY2), ModContent.DustType<LightBubble>(), new Vector2(0, 0));
					dust2.noGravity = true;
					dust2.scale = 1.2f;
					dust2 = Dust.NewDustDirect(new Vector2(position.X, position.Y), 0, 0, DustID.Smoke,
						-unit.X * Distance, -unit.Y * Distance);
					dust2.fadeIn = 0f;
					dust2.noGravity = true;
					dust2.scale = 0.88f;
					dust2.color = Color.Cyan;
				}

				if (increaseY2 < Distance)//Moving the spiral up
				{
					increaseY2 += 10f;
				}
			}

			#endregion SpiralDust
		}

		/// <summary>
		/// Sets the end of the laser position based on where it collides with something
		/// </summary>
		private void SetLaserPosition()
		{
			for (Distance = MOVE_DISTANCE; Distance <= 2500f; Distance += 1f)
			{
				var start = position + spriterotation * Distance;
				if (!Collision.CanHit(position, 1, 1, start, 1, 1))
				{
					if (!IsAtMaxCharge)
					{
						Distance -= 0f;
						break;
					}
					else if (IsAtMaxCharge && Projectile.timeLeft <= 80)
					{
						Distance -= 50f - offDistance;
						offDistance += 0.6f;
						break;
					}
					else
					{
						Distance -= 50f;
						break;
					}
				}
			}
		}

		private void ChargeLaser(Player player)
		{
			if (Charge < MAX_CHARGE)
			{
				Charge++;
			}
		}

		private void UpdatePlayer(Player player)
		{
			// Multiplayer support here, only run this code if the client running it is the owner of the Projectile
			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 diff = Main.MouseWorld - player.Center;
				diff.Normalize();
				Projectile.velocity = diff;
				Projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
				Projectile.netUpdate = true;
			}
			int dir = Projectile.direction;
			player.heldProj = Projectile.whoAmI; // Update player's held Projectile
		}

		private void CastLights()
		{
			// Cast a light along the line of the laser
			DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
			Utils.PlotTileLine(position, position + spriterotation * (Distance - MOVE_DISTANCE), 50, DelegateMethods.CastLight);
		}
	}
}
