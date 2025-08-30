using EBF.Abstract_Classes;
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
    public class Seraphim : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaultsSafe()
        {
            Item.width = 90;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 90;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 108;//Item's base damage value
            Item.knockBack = 1f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 36;//The amount of mana this item consumes on use

            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 20, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item8;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<Seraphim_Judgement>();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            player.FindSentryRestingSpot(type, out int XPosition, out int YPosition, out int _);
            position = new Vector2(XPosition, YPosition);
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
        public static readonly SoundStyle JudgementSound = new("EBF/Assets/Sfx/Judgement")
        {
            Volume = 0.8f,
            PitchVariance = 1f
        };

        //Fields
        private const int maxCharge = 40; //How long it takes for the beam to charge up
        private const int maxHeight = 2000; //How far the beam extends up, given it doesn't hit any tiles
        private float beamEdgeOffset = 0; //Displaces the head and tail of the beam based on their scale.
        private float beamScale = 5f; //Used for the animation illusion
        private float beamWidth = 100f; //Collision hitbox
        private int animation = 0; //Sets 0 or 1 for a small animation

        //Dust spiral
        private float increaseY = 0f; //Keeps track of the current height of the spiral dust
        private const float spiralWavelength = 70f; //How long one wave is, aka. the time it takes for the spiral to swap
        private const float spiralAmplitude = 100f; //How far from the center the wave travels

        //Properties
        private Vector2 Position { get => Projectile.position; set => Projectile.position = value; } //Base of the beam
        private static Vector2 Up { get => -Vector2.UnitY; }
        private float LaserHeight { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        private float Charge { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private bool IsAtMaxCharge => Charge == maxCharge;

        //Methods
        public override bool ShouldUpdatePosition() => false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = maxHeight;
        }
        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 120 + (int)maxCharge;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.localNPCHitCooldown = 7;
            Projectile.usesLocalNPCImmunity = true;

        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!IsAtMaxCharge) //When it's not at max charge have a small laser beam
            {
                beamScale = 1f;
            }
            else if (IsAtMaxCharge && Projectile.timeLeft <= 80) //if it's at max charge and some time has passed reduce its scale.
            {
                beamScale -= 0.06f;
            }
            else //The animation while damaging
            {
                if (animation == 0)
                {
                    beamScale = 5.5f;
                    animation = 1;
                }
                else if (animation == 1)
                {
                    beamScale = 5f;
                    animation = 0;
                }
            }
            if (beamScale <= 0f)
            {
                beamScale = 0f; //without this, the beam flips while the game is paused
                Projectile.Kill();
            }

            beamEdgeOffset = 10 * beamScale;

            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Position, step: 8, rotation: -1.57f, beamScale, Color.White, beamEdgeOffset);
            return false;
        }
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, float step, float rotation = 0f, float scale = 1f, Color color = default, float beamEdgeOffset = 0)
        {
            Vector2 bodyPosition;
            Vector2 origin = new Vector2(28 * 0.5f, 26 * 0.5f);
            float rot = Up.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = beamEdgeOffset; i <= LaserHeight; i += step)
            {
                bodyPosition = start + i * Up;
                color = i < beamEdgeOffset ? Color.Transparent : Color.White;
                spriteBatch.Draw(texture, bodyPosition - Main.screenPosition, new Rectangle(0, 26, 28, 26), color, rot, origin, scale, 0, 0);
            }

            // Draws the laser 'tail'
            spriteBatch.Draw(texture, start + Up * (beamEdgeOffset - step) - Main.screenPosition,
                new Rectangle(0, 0, 28, 26), color, rot, origin, scale, 0, 0);

            // Draws the laser 'head'
            spriteBatch.Draw(texture, start + Up * (LaserHeight + step) - Main.screenPosition,
                new Rectangle(0, 52, 28, 26), color, rot, origin, scale, 0, 0);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!IsAtMaxCharge)
            {
                return false;
            }
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Position, Position + Up * LaserHeight, beamWidth, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Spawn a projectile on the target
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<Seraphim_LightExplosion>(), Damage: 0, KnockBack: 0, Projectile.owner);
        }
        public override void AI()
        {
            //Beam Shrinking
            if (IsAtMaxCharge && Projectile.timeLeft <= 80)
            {
                beamWidth -= 0.5f; //also shrinks hitbox
            }
            else
            {
                beamWidth = 100f;
            }

            ChargeLaser();
            SetLaserHeight();
            SpawnDusts();
            CastLights();
        }
        private void SpawnDusts()
        {
            #region generalDust

            Dust dust;
            Vector2 dustVel = new Vector2(Main.rand.NextBool(2) ? -1 : 1, 0);

            //Electric dust
            dust = Dust.NewDustDirect(Position, 0, 0, DustID.Electric, dustVel.X * 10, dustVel.Y * 10, 0, newColor: Color.White, Scale: 1.2f);
            dust.noGravity = true;
            dust.shader = GameShaders.Armor.GetSecondaryShader(64, Main.LocalPlayer);

            //Smoke dust
            dust = Dust.NewDustDirect(Position, 0, 0, DustID.Smoke, Position.X * LaserHeight, Position.Y * LaserHeight, newColor: Color.White, Scale: 0.88f);
            dust.noGravity = true;
            dust.shader = GameShaders.Armor.GetSecondaryShader(64, Main.LocalPlayer);


            #endregion generalDust

            #region Feathers

            if (IsAtMaxCharge && Projectile.localAI[2] == 0)
            {
                Projectile.localAI[2] = 1; //Ensure this only happens once

                if (!Main.dedServ)
                    SoundEngine.PlaySound(JudgementSound, Projectile.Center);

                for (int i = 0; i < 85; ++i)
                {
                    dustVel = new Vector2(1, 0).RotatedBy(Main.rand.NextFloat(1.57f, 2.57f) + (Main.rand.NextBool(2) ? -1.6f : 1.0f) * 1.57f);

                    float rand = Main.rand.NextFloat(5f, 20f);

                    //Feather dust
                    dust = Dust.NewDustDirect(Position, 0, 0, ModContent.DustType<LightFeather>(), dustVel.X * rand, dustVel.Y * rand, Alpha: 2, Scale: 1.2f);
                    dust.noGravity = true;
                    dust.noLight = true;

                    //Smoke dust
                    dust = Dust.NewDustDirect(Position, 0, 0, DustID.Smoke, Position.X * LaserHeight, Position.Y * LaserHeight, Alpha: 2, newColor: Color.Cyan, Scale: 0.88f);
                    dust.noGravity = true;
                }
            }

            #endregion Feathers

            #region SpiralDust

            if (IsAtMaxCharge && Main.GameUpdateCount % 2 == 0)
            {
                Vector2 dustPosition;

                //Bubble dust 1
                dustPosition = new Vector2((float)(Position.X + (spiralAmplitude * Math.Sin(increaseY / spiralWavelength))), Position.Y - increaseY);
                dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<LightBubble>(), Vector2.Zero, Scale: 1.2f);
                dust.noGravity = true;

                //Bubble dust 2
                dustPosition = new Vector2((float)(Position.X - (spiralAmplitude * Math.Sin(increaseY / spiralWavelength))), Position.Y - increaseY);
                dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<LightBubble>(), Vector2.Zero, Scale: 1.2f);
                dust.noGravity = true;

                //Smoke dust 1
                dust = Dust.NewDustDirect(Position, 0, 0, DustID.Smoke, Position.X * LaserHeight, Position.Y * LaserHeight, newColor: Color.Cyan, Scale: 0.88f);
                dust.noGravity = true;

                //Smoke dust 2
                dust = Dust.NewDustDirect(Position, 0, 0, DustID.Smoke, Position.X * LaserHeight, Position.Y * LaserHeight, newColor: Color.Cyan, Scale: 0.88f);
                dust.noGravity = true;

                //Move the spiral up
                if (increaseY < LaserHeight)
                {
                    increaseY += 20;
                }
            }

            #endregion SpiralDust
        }
        private void SetLaserHeight()
        {
            //Limit how often this method runs for performance
            if (Main.GameUpdateCount % 4 != 0)
                return;

            //Check each tile up from the start of the beam
            for (LaserHeight = beamEdgeOffset; LaserHeight <= maxHeight; LaserHeight += 16f)
            {
                Vector2 bodyPosition = Position + Up * LaserHeight;
                if (!Collision.CanHit(Position, 1, 1, bodyPosition , 1, 1))
                {
                    LaserHeight -= 16f + beamEdgeOffset;
                    break;
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
        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Position, Position + Up * (LaserHeight - beamEdgeOffset), 50, DelegateMethods.CastLight);
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
