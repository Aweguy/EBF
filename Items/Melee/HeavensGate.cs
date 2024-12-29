using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class HeavensGate : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Heaven's Gate");//Name of the Item
            base.Tooltip.WithFormatArgs("A legendary sword belonging to a line of famed corsairs.");//Tooltip of the item
        }
        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 49;//Item's base damage value
            Item.knockBack = 2f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 34;//How fast the item is used
            Item.useAnimation = 34;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 9, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<HeavensGate_LightBlade>();
            Item.shootSpeed = 5f;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.AncientLight);
                Main.dust[dust].noGravity = true;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Spawn sword between cursor and player
            position = Main.MouseWorld - (Vector2.Normalize(velocity) * 80f);

            //Save velocities to be used by child swords
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, velocity.X, velocity.Y);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Excalibur, stack: 1)
                .AddIngredient(ItemID.Ectoplasm, stack: 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /* TODO: Merge both of these projectiles into one
     */

    public class HeavensGate_LightBlade : ModProjectile
    {
        private const float spawnPositionOffset = 80f;
        private bool stop = false;
        private Vector2 spawnPosition;
        private Vector2 moveSpeed; //Stores the default velocity so the info isn't lost when the projectile stops
        private int trailSkip = 2;

        public override void SetStaticDefaults()//Mainly used for setting the frames of animations or things we don't want to change in the projectile
        {
            Main.projFrames[Projectile.type] = 11;
            ProjectileID.Sets.TrailingMode[Type] = 2; // Creates a trail behind the golf ball.
            ProjectileID.Sets.TrailCacheLength[Type] = 36; // Sets the length of the trail.
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.knockBack = 7f;
            Projectile.light = 1f;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.scale = 1.3f;
        }
        public override void OnKill(int timeLeft)
        {
            Vector2 dustPosition = Projectile.position;
            Vector2 dustOldVelocity = Vector2.Normalize(Projectile.oldVelocity);
            dustPosition += dustOldVelocity * 16f;
            for (int i = 0; i < 20; i++)
            {
                int light = Dust.NewDust(dustPosition, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 0, default(Color), 1f);
                Main.dust[light].position = (Main.dust[light].position + Projectile.Center) / 2f;

                Dust dust = Main.dust[light];
                dust.velocity += Projectile.oldVelocity * 0.6f;
                dust = Main.dust[light];
                dust.velocity *= 0.5f;

                Main.dust[light].noGravity = true;
                dustPosition -= dustOldVelocity * 8f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Randomize direction
            float rotation = Main.rand.NextFloat((float)Math.Tau);
            Vector2 velocity = Projectile.velocity.RotatedBy(rotation);
            Vector2 position = target.Center - (Vector2.Normalize(velocity) * 80f);

            //Spawn projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<HeavensGate_LightBlade_Mini>(), hit.Damage, Projectile.knockBack, Projectile.owner, target.whoAmI, Projectile.whoAmI);
        }
        public override bool? CanDamage() //If it's not fully form, do not damage
        {
            return Projectile.frame == 4;
        }
        public override void OnSpawn(IEntitySource source)
        {
            spawnPosition = Main.MouseWorld - Vector2.Normalize(new Vector2(Projectile.ai[0], Projectile.ai[1])) * spawnPositionOffset;
            moveSpeed = new Vector2(Projectile.ai[0], Projectile.ai[1]);
        }
        public override bool PreAI()//Use this to write the AI of the projectile. Its behaviour in other words. Updates every frame.
        {
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                Main.dust[dust].velocity.X *= 0.4f;
                Main.dust[dust].noGravity = true;
            }

            if (!stop)
            {
                HandleFrames();
            }
            else if (Vector2.Distance(spawnPosition, Projectile.Center) >= spawnPositionOffset * 4f)
            {
                stop = false;
            }

            float velRotation = moveSpeed.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f); //necessary cuz sprite faces the wrong way
            Projectile.spriteDirection = Projectile.direction;
            return false;
        }
        private void HandleFrames()
        {
            //Advance frames every third tick
            if (++Projectile.frameCounter < 3)
            {
                return;
            }

            Projectile.frame++;
            Projectile.frameCounter = 0;

            if (Projectile.frame <= 3)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }
            else if (Projectile.frame == 4)
            {
                Projectile.velocity = Vector2.Normalize(moveSpeed) * 16f;
                stop = true;
            }
            else if (Projectile.frame > 4)
            {
                Projectile.velocity = Vector2.Zero;

                Projectile.netUpdate = true;

                if (Projectile.frame == 11)
                {
                    Projectile.Kill();
                }
            }
        }

        /*public override bool PreDraw(ref Color lightColor)
        {


            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            float initialOpacity = 0.8f;
            float opacityDegrade = 0.08f;



            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 1)
            {
                if(TrailSkip++ <= 0)
                {
                    float opacity = initialOpacity - opacityDegrade * i;
                    Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Hitbox.Size() / 2 - Main.screenPosition, frame, lightColor * opacity, Projectile.rotation, origin, Projectile.scale, effects, 0f);
                    TrailSkip = 2;
                }
            }

            #region Trailing

            #endregion Trailing
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin();

            return false;
        }*/
    }

    public class HeavensGate_LightBlade_Mini : ModProjectile
    {
        private const int copyLimit = 2; //How many times this projectile can be copied
        private const float spawnPositionOffset = 80f; //How far away new projectiles spawn from a target
        private bool stop = false;
        private Vector2 spawnPosition;
        private Vector2 moveSpeed; //Stores the default velocity so the info isn't lost when the projectile stops
        private Projectile parent;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.light = 1f;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.scale = 1.3f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (parent.localAI[0] < copyLimit)
            {
                Projectile.localAI[0] = parent.localAI[0] + 1;

                //Ensure only one projectile is spawned from this one
                if (++Projectile.localAI[1] == 1)
                {
                    //Randomize direction
                    float rotation = Main.rand.NextFloat((float)Math.Tau);
                    Vector2 velocity = Projectile.velocity.RotatedBy(rotation);
                    Vector2 position = target.Center - (Vector2.Normalize(velocity) * 80f);

                    //Spawn projectile
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<HeavensGate_LightBlade_Mini>(), hit.Damage, Projectile.knockBack, Projectile.owner, target.whoAmI, Projectile.whoAmI);
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            Vector2 dustPosition = Projectile.position;
            Vector2 dustOldVelocity = Vector2.Normalize(Projectile.oldVelocity);
            dustPosition += dustOldVelocity * 16f;
            for (int i = 0; i < 20; i++)
            {
                int light = Dust.NewDust(dustPosition, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 0, default(Color), 1f);
                Main.dust[light].position = (Main.dust[light].position + Projectile.Center) / 2f;

                Dust dust = Main.dust[light];
                dust.velocity += Projectile.oldVelocity * 0.6f;
                dust = Main.dust[light];
                dust.velocity *= 0.5f;

                Main.dust[light].noGravity = true;
                dustPosition -= dustOldVelocity * 8f;
            }
        }
        public override bool? CanDamage() //If it's not fully form, do not damage
        {
            return Projectile.frame == 4;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC target = Main.npc[(int)Projectile.ai[0]];
            parent = Main.projectile[(int)Projectile.ai[1]];

            spawnPosition = target.Center - Vector2.Normalize(Projectile.velocity) * spawnPositionOffset;
            moveSpeed = Projectile.velocity;
        }
        public override bool PreAI()
        {

            //Change the number to determine how much dust will spawn. lower for more, higher for less
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                Main.dust[dust].velocity.X *= 0.4f;
            }

            if (!stop)
            {
                HandleFrames();
            }
            else if (Vector2.Distance(spawnPosition, Projectile.Center) >= spawnPositionOffset * 4f)
            {
                stop = false;
            }

            float velRotation = moveSpeed.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f); //necessary cuz sprite faces the wrong way
            Projectile.spriteDirection = Projectile.direction;

            return false;
        }
        private void HandleFrames()
        {
            //Advance frames every third tick
            if (++Projectile.frameCounter < 3)
            {
                return;
            }

            Projectile.frame++;
            Projectile.frameCounter = 0;

            if (Projectile.frame <= 3)
            {
                Projectile.velocity = Vector2.Zero;
            }
            else if (Projectile.frame == 4)
            {
                Projectile.velocity = moveSpeed;
                stop = true;
            }
            else if (Projectile.frame > 4)
            {
                Projectile.velocity = Vector2.Zero;

                if (Projectile.frame == 11)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
