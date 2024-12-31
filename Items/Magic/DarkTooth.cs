using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class DarkTooth : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Dark Tooth");//Name of the Item
            base.Tooltip.WithFormatArgs("Ancient black magic staff used for Dark elemental magic. Creates a slowly growing black hole that explodes afterwards.\nPulls everything towards it, even the player\nConsumes Limit Break while active");//Tooltip of the item
        }
        public override void SetDefaults()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)
            Item.scale = 1.5f;

            Item.damage = 40;//Item's base damage value
            Item.knockBack = 0;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 50;//How fast the item is used
            Item.useAnimation = 50;//How long the animation lasts. For swords it should stay the same as UseTime
            Item.channel = true;//Channeling the item when held

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Purple;//Item's name colour, this is hardcoded by the modder and should be based on 
            Item.UseSound = SoundID.Item88;//The item's sound when it's used
            Item.autoReuse = false;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<DarkTooth_BlackHole>();
            Item.shootSpeed = 0.1f; //Must be > 0 to make the held item rotate when used
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation -= new Vector2(player.direction * 16, 2);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }
        public override void HoldItem(Player player)
        {
            Color drawColor = Main.rand.NextBool(2) ? Color.Black : Color.Red;

            if (player.channel)
            {
                Dust.NewDustDirect(player.position, player.width, player.height, DustID.Terragrim, newColor: drawColor, Scale: 1f);
            }
        }
        public override bool CanUseItem(Player player)
        {
            //Cannot use if the player's old black hole exists
            return player.ownedProjectileCounts[ModContent.ProjectileType<DarkTooth_BlackHole>()] < 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, stack: 30)
                .AddIngredient(ItemID.Ruby, stack: 5)
                .AddIngredient(ItemID.SoulofNight, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class DarkTooth_BlackHole : ModProjectile
    {
        private const int spawningDust = 100; //How much dust will be created when the projectile spawns
        private const int dustSpawnRate = 100; //bigger = more dust, total dust is dustSpawnTime * dustSpawnRate
        private const float dustBoost = 2f; //The offset from the center from which the dust will spawn
        private const float defaultSuckRange = 160;//The default range in which objects will be SUCCED
        private const float maxSize = 400f;
        private float gravMagnitude; //The power of the gravitational force
        private float suckRange = 160;//The current range in which objects will be SUCCED
        private int baseWidth;
        private int baseHeight;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.knockBack = 1f;

            Projectile.timeLeft = 60 * 100000;
            Projectile.tileCollide = false;

            baseWidth = Projectile.width;
            baseHeight = Projectile.height;
        }
        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            if (Main.GameUpdateCount % 5 == 0) //Run this code every 5 frames
            {
                //When the black hole first appears, spawn a bunch of dust
                if (Projectile.frame == 8)
                {
                    CreateSpawningDust();
                }

                Projectile.frame++;
                if (Projectile.frame >= 10)
                {
                    //Spawn dust passively
                    Color drawColor = Main.rand.NextBool(2) ? Color.Black : Color.Red;
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, newColor: drawColor, Scale: 1f);

                    // Mouse position can vary in multiplayer, so this code must only run on the client
                    if (Main.myPlayer == Projectile.owner)
                    {
                        MoveTowardsCursor();//The movement of the black hole

                        // If the player channels the weapon, do something. This check only works if item.channel is true for the weapon.
                        if (player.channel)
                        {
                            suckRange = defaultSuckRange * Projectile.scale;
                            if (suckRange > 1600f)
                            {
                                suckRange = 1600f;
                            }
                            IncreaseScale(player, Projectile.Size);//The growth of the black hole
                            SuckNPCs(suckRange);
                            SuckGore(suckRange);
                            SuckDust(suckRange);
                        }
                        // If the player stops channeling, do something else.
                        else
                        {
                            Projectile.timeLeft = 1;

                            Damage();//The method that calculates the damage
                        }
                    }

                    //Loop animation
                    if (Projectile.frame >= 16)
                    {
                        Projectile.frame = 10;
                    }
                }
            }
            return false;
        }
        public override void OnKill(int timeLeft) //The dust when the Projectile dies
        {

            if (Projectile.width <= 150)
            {
                for (int i = 0; i < 25; i++)
                {
                    int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 1f, 1f, 0, Color.Red, 1f);
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
                    dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 1f, 1f, 0, Color.Black, 1.25f);
                    Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
                }
            }
            else if (Projectile.width <= 325 && Projectile.width > 150)
            {
                for (int i = 0; i < 50; i++)
                {
                    int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 1f, 1f, 0, Color.Red, 1.5f);
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
                    dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 1f, 1f, 0, Color.Black, 2f);
                    Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
                }
            }
            else
            {
                for (int i = 0; i < 200; i++)
                {
                    int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 1f, 1f, 0, Color.Red, 2f);
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
                    dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 1f, 1f, 0, Color.Black, 2.5f);
                    Main.dust[dustIndex].velocity += Vector2.Normalize(Main.dust[dustIndex].position - Projectile.Center) * 10;
                }
            }

            // Large Smoke Gore spawn
            // reset size to normal width and height.
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
        }
        public override bool PreDraw(ref Color lightColor) //Code for making thte Projectile animate while its position is centered
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // This is where the magic happens.
            int frameWidth = texture.Width / 2;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];

            int frameX = (int)(Projectile.frame / Main.projFrames[Projectile.type]) * frameWidth;
            int frameY = (Projectile.frame % Main.projFrames[Projectile.type]) * frameHeight;
            Rectangle frame = new Rectangle(frameX, frameY, frameWidth, frameHeight);
            // This is where the magic stops :(

            Vector2 origin = frame.Size() / 2;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Do not allow vanilla drawing code to execute.
            return (false);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        private void CreateSpawningDust()
        {
            for (int i = 0; i < spawningDust; i++)
            {
                float rot = Main.rand.NextFloat(0, (float)Math.Tau); //random angle
                float speed = Main.rand.NextFloat(3f, 10f);

                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * dustBoost * 20f;
                Vector2 velocity = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * speed;

                Dust dust = Dust.NewDustPerfect(position, 31, velocity, 0, new Color(255, 255, 255), 1f);
                dust.noGravity = true;
            }
        }
        private void MoveTowardsCursor() //Movement of the black hole
        {
            float maxDistance = 3f; // This also sets the maximun speed the Projectile can reach while following the cursor.
            Vector2 vectorToCursor = Main.MouseWorld - Projectile.Center;
            float distanceToCursor = vectorToCursor.Length();

            // Here we can see that the speed of the Projectile depends on the distance to the cursor.
            if (distanceToCursor > maxDistance)
            {
                distanceToCursor = maxDistance / distanceToCursor;
                vectorToCursor *= distanceToCursor;
            }

            int velocityXBy1000 = (int)(vectorToCursor.X * 3f);
            int oldVelocityXBy1000 = (int)(Projectile.velocity.X * 3f);
            int velocityYBy1000 = (int)(vectorToCursor.Y * 3f);
            int oldVelocityYBy1000 = (int)(Projectile.velocity.Y * 3f);

            // This code checks if the precious velocity of the Projectile is different enough from its new velocity, and if it is, syncs it with the server and the other clients in MP.
            // We previously multiplied the speed by 1000, then casted it to int, this is to reduce its precision and prevent the speed from being synced too much.
            if (velocityXBy1000 != oldVelocityXBy1000 || velocityYBy1000 != oldVelocityYBy1000)
            {
                Projectile.netUpdate = true;
            }

            Projectile.velocity = vectorToCursor;
        }
        private void IncreaseScale(Player player, Vector2 oldSize) //The growth rate of the black hole
        {
            if (Projectile.width < maxSize)
            {
                Projectile.scale += 0.05f;
                Projectile.width = (int)(baseWidth * Projectile.scale);
                Projectile.height = (int)(baseHeight * Projectile.scale);
                Projectile.position = Projectile.position - (Projectile.Size - oldSize) / 2f;
            }
        }
        private void SuckNPCs(float suckingRange)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active)
                {
                    float between = Vector2.Distance(Projectile.Center, npc.Center);//Calculating the distance

                    if (between < 0.1f)
                    {
                        between = 0.1f;
                    }
                    gravMagnitude = (Projectile.scale * 75) / between; //gravitaional pull equation

                    if (!npc.boss && between <= suckingRange)
                    {
                        npc.velocity += npc.DirectionTo(Projectile.Center) * gravMagnitude;//applying the gravitational pull force calculated above
                    }
                }
            }
        }
        private void SuckGore(float suckingRange)
        {
            for (int i = 0; i < Main.maxGore; i++)
            {
                Gore gore = Main.gore[i];
                if (gore.active)
                {
                    float between = Vector2.Distance(Projectile.Center, gore.position);

                    if (between < 0.1f)
                    {
                        between = 0.1f;
                    }
                    if (between <= suckingRange)
                    {
                        gravMagnitude = (Projectile.scale * 100) / between; //gravitaional pull equation
                        gore.velocity -= Vector2.Normalize(gore.position - Projectile.Center) * gravMagnitude;//Final Calculation
                    }
                }
            }
        }
        private void SuckDust(float suckingRange)
        {
            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (dust.active)
                {
                    float between = Vector2.Distance(Projectile.Center, dust.position);

                    if (between < 0.1f)
                    {
                        between = 0.1f;
                    }
                    if (between <= suckingRange)
                    {
                        gravMagnitude = (Projectile.scale * 100) / between; //gravitaional pull equation
                        dust.velocity -= Vector2.Normalize(dust.position - Projectile.Center) * gravMagnitude;//Final Calculation
                    }
                }
            }
        }
        private void Damage() //Damage after it blows up
        {
            Projectile.tileCollide = false;
            // Set to transparent. This Projectile technically lives as  transparent for about 3 frames
            // change the hitbox size, centered about the original Projectile center. This makes the Projectile damage enemies during the explosion.
            Projectile.position = Projectile.Center;
            //Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            //Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            if (Projectile.width <= 150)
            {
                Projectile.width += 80;
                Projectile.height += 80;
                Projectile.damage = (80 + Projectile.width) * 3;
            }
            else if (Projectile.width <= 325 && Projectile.width > 150)
            {
                Projectile.width += 220;
                Projectile.height += 220;
                Projectile.damage = (220 + Projectile.width) * 4;
            }
            else
            {
                Projectile.width += 500;
                Projectile.height += 500;
                Projectile.damage = (700 + Projectile.width) * 5;
            }
            Projectile.Center = Projectile.position;
            //Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            //Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
        }
    }
}
