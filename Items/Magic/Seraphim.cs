using EBF.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

            Item.damage = 52;//Item's base damage value
            Item.knockBack = 0f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 36;//The amount of mana this item consumes on use
            Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 20, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item8;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<Seraphim_Judgement>();
            Item.shootSpeed = 1f;//The held item requires shootSpeed > 0 in order to rotate on use.
            Item.noMelee = true;//Prevents damage from being dealt by the item itself
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation -= new Vector2(player.direction * 16, 2);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.FindSentryRestingSpot(type, out int XPosition, out int YPosition, out int YOffset);

            position = new Vector2(XPosition, YPosition);
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }
        public override bool CanUseItem(Player player)
        {
            //Can't use it if your judgement still exists
            return player.ownedProjectileCounts[ModContent.ProjectileType<Seraphim_Judgement>()] < 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.SpectreBar, stack: 26)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class Seraphim_Judgement : ModProjectile
    {
        public static readonly SoundStyle JudgementSound = new("EBF/Assets/Sounds/Custom/Judgement")
        {
            Volume = 1f,
            PitchVariance = 1f
        };

        #region Variables and Constants

        //Fields
        private const float maxCharge = 40f;
        public float beamVerticalOffset = 20f; //The distance charge particle from the player center
        public float laserHeight; //This is how tall the laser is, it's set in SetLaserHeight

        private float beamScale = 5f;//Used for the animation illusion
        private float increaseY = 0f; //It increases the Y axis of the dust spawning
        private const float waveFrequency = 70f;//Dust spawning wave frequency on both spawners
        private const float waveLength = 100f;//Dust Spawning wave length on both spawners
        private float beamWidth = 100f;//Collision hitbox.
        private float beamHeadVerticalOffset = 0.6f;//Distance Reduction

        private Vector2 position;//the initial position of the laser
        private Vector2 spriterotation = -Vector2.UnitY;//rotation of the laser to look up

        private int animation = 0;//Sets 0 or 1 for a small animation.

        //Properties
        public float Charge //Shortcut for readability
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public bool IsAtMaxCharge => Charge == maxCharge;

        #endregion

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 120 + (int)maxCharge;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.hide = true;

            Projectile.localNPCHitCooldown = 7;
            Projectile.usesLocalNPCImmunity = true;

        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!IsAtMaxCharge)//When it's not at max charge have a small laser beam
            {
                beamScale = 1f;
                beamVerticalOffset = 4f;
            }
            else if (IsAtMaxCharge && Projectile.timeLeft <= 80)//if it's at max charge and some time has passed reduce its scale.
            {
                beamScale -= 0.06f;
                beamVerticalOffset -= 0.24f;
            }
            else//The animation while damaging
            {
                if (animation == 0)
                {
                    beamScale = 5.5f;
                    animation = 1;
                    beamVerticalOffset = 20f;
                }
                else if (animation == 1)
                {
                    beamScale = 5f;
                    animation = 0;
                    beamVerticalOffset = 20f;
                }
            }
            if (beamScale <= 0f)
            {
                Projectile.Kill();
            }

            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, position, spriterotation, step: 8, rotation: -1.57f, beamScale, Color.White, (int)beamVerticalOffset);
            return false;
        }
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, float rotation = 0f, float scale = 1f, Color color = default(Color), int beamVerticalOffset = 0)
        {
            Vector2 bodyPosition;
            Vector2 origin = new Vector2(28 * 0.5f, 26 * 0.5f);
            float rot = unit.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = beamVerticalOffset; i <= laserHeight; i += step)
            {
                bodyPosition = start + i * unit;
                color = i < beamVerticalOffset ? Color.Transparent : Color.White;
                spriteBatch.Draw(texture, bodyPosition - Main.screenPosition, new Rectangle(0, 26, 28, 26), color, rot, origin, scale, 0, 0);
            }

            // Draws the laser 'tail'
            spriteBatch.Draw(texture, start + unit * (beamVerticalOffset - step) - Main.screenPosition,
                new Rectangle(0, 0, 28, 26), color, rot, origin, scale, 0, 0);

            // Draws the laser 'head'
            spriteBatch.Draw(texture, start + (laserHeight + step) * unit - Main.screenPosition,
				new Rectangle(0, 52, 28, 26), color, rotation: (float)Math.PI, origin, scale, 0, 0);
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
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, position + unit * laserHeight, beamWidth, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Spawn a projectile on the target
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<Seraphim_LightExplosion>(), Damage: 0, KnockBack: 0, Projectile.owner);
        }
        public override void OnSpawn(IEntitySource source)
        {
            position = Projectile.position;
        }
        public override void AI()
        {
            //Beam Shrinking
            if (IsAtMaxCharge && Projectile.timeLeft <= 80)
            {
                beamWidth -= 0.5f; //also reduces the hitbox
            }
            else
            {
                beamWidth = 100f;
            }

            // By separating large AI into methods it becomes very easy to see the flow of the AI in a broader sense
            // First we update player variables that are needed to channel the laser
            // Then we run our charging laser logic
            // If we are fully charged, we proceed to update the laser's position
            // Finally we spawn some effects like dusts and light

            Player player = Main.player[Projectile.owner];
            UpdatePlayer(player);
            ChargeLaser();

            SetLaserHeight();
            SpawnDusts();
            CastLights();
        }
        private void SpawnDusts()
        {
            Vector2 unit = position;

            #region generalDust

            for (int i = 0; i < 1; ++i)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextBool(2) ? -1 : 1, 0);

                //Electric dust
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.Electric, dustVel.X * 10, dustVel.Y * 10, 0, newColor: Color.White, Scale: 1.2f);
                dust.noGravity = true;
                dust.shader = GameShaders.Armor.GetSecondaryShader(64, Main.LocalPlayer);

                //Smoke dust
                dust = Dust.NewDustDirect(position, 0, 0, DustID.Smoke, unit.X * laserHeight, unit.Y * laserHeight, newColor: Color.White, Scale: 0.88f);
                dust.noGravity = true;
                dust.shader = GameShaders.Armor.GetSecondaryShader(64, Main.LocalPlayer);
            }

            #endregion generalDust

            #region Feathers

            if (IsAtMaxCharge && Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] = 1; //Ensure this only happens once

                if (!Main.dedServ)
                    SoundEngine.PlaySound(JudgementSound, Projectile.Center);

                for (int i = 0; i < 85; ++i)
                {
                    Vector2 dustVel = new Vector2(1, 0).RotatedBy(Main.rand.NextFloat(1.57f, 2.57f) + (Main.rand.NextBool(2) ? -1.6f : 1.0f) * 1.57f);

                    float rand = Main.rand.NextFloat(5f, 20f);

                    //Feather dust
                    Dust dust = Dust.NewDustDirect(position, 0, 0, ModContent.DustType<LightFeather>(), dustVel.X * rand, dustVel.Y * rand, Alpha: 2, Scale: 1.2f);
                    dust.noGravity = true;
                    dust.noLight = true;

                    //Smoke dust
                    dust = Dust.NewDustDirect(position, 0, 0, DustID.Smoke, unit.X * laserHeight, unit.Y * laserHeight, Alpha: 2, newColor: Color.Cyan, Scale: 0.88f);
                    dust.noGravity = true;
                }
            }

            #endregion Feathers

            #region SpiralDust

            if (IsAtMaxCharge && Main.GameUpdateCount % 2 == 0)
            {
                Vector2 dustPosition;

                //Bubble dust 1
                dustPosition = new Vector2((float)(position.X + (waveLength * Math.Sin(increaseY / waveFrequency))), position.Y - increaseY);
                Dust dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<LightBubble>(), Vector2.Zero, Scale: 1.2f);
                dust.noGravity = true;

                //Bubble dust 2
                dustPosition = new Vector2((float)(position.X - (waveLength * Math.Sin(increaseY / waveFrequency))), position.Y - increaseY);
                Dust dust2 = Dust.NewDustPerfect(dustPosition, ModContent.DustType<LightBubble>(), Vector2.Zero, Scale: 1.2f);
                dust2.noGravity = true;

                //Smoke dust 1
                dust = Dust.NewDustDirect(position, 0, 0, DustID.Smoke, unit.X * laserHeight, unit.Y * laserHeight, newColor: Color.Cyan, Scale: 0.88f);
                dust.noGravity = true;

                //Smoke dust 2
                dust2 = Dust.NewDustDirect(position, 0, 0, DustID.Smoke, unit.X * laserHeight, unit.Y * laserHeight, newColor: Color.Cyan, Scale: 0.88f);
                dust2.noGravity = true;

                //Move the spiral up
                if (increaseY < laserHeight)
                {
                    increaseY += 20;
                }
            }

            #endregion SpiralDust
        }
        private void SetLaserHeight()
        {
            //Check each tile up from the start of the beam
            for (laserHeight = beamVerticalOffset; laserHeight <= 2500f; laserHeight += 16f)
            {
                Vector2 bodyPosition = position + spriterotation * laserHeight;
                if (!Collision.CanHit(position, 1, 1, bodyPosition, 1, 1))
                {
                    if (!IsAtMaxCharge)
                    {
                        laserHeight -= 0f;
                        break;
                    }
                    else 
                    {
                        laserHeight -= 16f;
                        break;
                    }
                }
            }
        }
        private void ChargeLaser()
        {
            if (Charge < maxCharge)
            {
                Charge++;
            }
        }
        private void UpdatePlayer(Player player)
        {
            // Multiplayer support here, only run this code if the client running it is the owner of the Projectile
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center);
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
            Utils.PlotTileLine(position, position + spriterotation * (laserHeight - beamVerticalOffset), 50, DelegateMethods.CastLight);
        }
    }

    public class Seraphim_LightExplosion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;

            Projectile.alpha = 1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override bool PreAI()
        {
            if (++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 7)
                {
                    Projectile.Kill();
                }
            }

            return false;
        }
    }
}
