using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.GameContent;
using EBF.EbfUtils;

namespace EBF.Items.Magic
{
    public class DarkTooth : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        int manaDrainTimer; //Used to reduce how often mana is drained
        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 40;//Item's base damage value
            Item.knockBack = 0;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 40;//The amount of mana this item consumes on use

            Item.useTime = 50;//How fast the item is used
            Item.useAnimation = 50;//How long the animation lasts. For swords it should stay the same as UseTime
            Item.channel = true;//Channeling the item when held

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on 
            Item.UseSound = SoundID.Item88;//The item's sound when it's used
            Item.autoReuse = false;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<DarkTooth_BlackHole>();
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

                DrainMana(player);
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
        private void DrainMana(Player player)
        {
            manaDrainTimer++;
            if (manaDrainTimer > 10)
            {
                manaDrainTimer = 0;

                int manaCost = 4;
                if (player.statMana >= manaCost)
                {
                    player.statMana -= manaCost;
                    player.manaRegenDelay = 60; //Reset mana regen
                }
                else
                {
                    player.channel = false; //Stop channeling if out of mana
                }
            }
        }
    }

    public class DarkTooth_BlackHole : ModProjectile
    {
        private const int spawningDust = 100; //How much dust will be created when the projectile spawns
        private const int endingDust = 50; //How much dust will be created when the projectile dies
        private const float dustBoost = 2f; //The offset from the center from which the dust will spawn
        private const float defaultSuckRange = 160;//The default range in which objects will be SUCCED
        private const float maxSize = 400f;
        private const float maxSpeed = 5f;
        private float currentSpeed = 0f;
        private float suckRange;//The current range in which objects will be SUCCED
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

            Projectile.friendly = false;
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
            //Run this code once every 8 updates
            if (Main.GameUpdateCount % 8 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame == 8)
                {
                    //When the black hole first appears, spawn a bunch of dust
                    CreateSpawningDust();
                    Projectile.friendly = true;
                }

                //Loop animation
                if (Projectile.frame >= 19)
                {
                    Projectile.frame = 10;
                }

                //Rotate randomly
                if (Projectile.frame > 9)
                {
                    Projectile.rotation = Main.rand.NextFloat(0, (float)Math.Tau);
                }
            }

            //While active
            if (Projectile.frame >= 10)
            {
                //Spawn dust passively
                Color drawColor = Main.rand.NextBool(2) ? Color.Black : Color.Red;
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, newColor: drawColor);

                // Mouse position can vary in multiplayer, so this code must only run on the client
                if (Main.myPlayer == Projectile.owner)
                {
                    HandleAudioLoop();
                    MoveTowardsCursor();//The movement of the black hole

                    //This check only works if item.channel is true for the weapon.
                    Player player = Main.player[Projectile.owner];
                    if (player.channel)
                    {
                        IncreaseScale();
                        SuckNPCs(suckRange, suckingStrength: 20);
                        SuckGore(suckRange, suckingStrength: 20);
                        SuckDust(suckRange, suckingStrength: 20);
                    }
                    else
                    {
                        Explode();
                    }
                }
            }
            return false;
        }
        public override void OnKill(int timeLeft) //The dust when the Projectile dies
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            //scale goes from 1 up to maxSize / 128
            for (int i = 0; i < endingDust * Projectile.scale; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, DustID.Smoke, 1f, 1f, newColor: Main.rand.NextBool(2) ? Color.Red : Color.Black, Scale: Projectile.scale);
                dust.velocity = Vector2.Normalize(dust.position - Projectile.Center) * 10;
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor) //Code for making the Projectile animate while its position is centered
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            int frameCount = Main.projFrames[Projectile.type];
            int frameWidth = texture.Width / 2;
            int frameHeight = texture.Height / frameCount;

            //This here is some mysterious shit
            int frameX = Projectile.frame / frameCount * frameWidth;
            int frameY = Projectile.frame % frameCount * frameHeight;

            Rectangle frame = new Rectangle(frameX, frameY, frameWidth, frameHeight);
            Vector2 origin = frame.Size() / 2;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
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
            //Gradually build up speed
            if(currentSpeed < maxSpeed)
            {
                currentSpeed += 0.1f;
            }

            //Set direction
            Vector2 newVelocity = Main.MouseWorld - Projectile.Center;
            if (newVelocity.Length() > currentSpeed)
            {
                newVelocity = Vector2.Normalize(newVelocity) * currentSpeed;
            }

            //Limit how often velocity syncs in multiplayer by truncating the decimals
            if (newVelocity.ToPoint() != Projectile.velocity.ToPoint())
            {
                Projectile.netUpdate = true;
            }

            Projectile.velocity = newVelocity;
        }
        private void IncreaseScale() //The growth rate of the black hole
        {
            if (Projectile.width < maxSize)
            {
                Projectile.scale += 0.01f;
                suckRange = defaultSuckRange * Projectile.scale;
                int newWidth = (int)(baseWidth * Projectile.scale);
                int newHeight = (int)(baseHeight * Projectile.scale);
                Projectile.ExpandHitboxTo(newWidth, newHeight);
            }
        }
        private void SuckNPCs(float suckingRange, float suckingStrength = 20)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.boss || npc.immortal || npc.dontTakeDamage) //immortal is target dummy
                {
                    continue;
                }

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= suckingRange)
                {
                    float gravityMagnitude = Projectile.scale * suckingStrength / (dist * 0.5f + 10f); //Won't divide by 0 :)
                    npc.velocity += npc.DirectionTo(Projectile.Center) * gravityMagnitude;
                }
            }
        }
        private void SuckGore(float suckingRange, float suckingStrength = 20)
        {
            foreach (Gore gore in Main.gore)
            {
                if (!gore.active)
                {
                    continue;
                }

                float dist = Vector2.Distance(Projectile.Center, gore.position);
                if (dist <= suckingRange)
                {
                    float gravityMagnitude = Projectile.scale * suckingStrength / (dist * 0.5f + 10f); // Won't divide by 0 :)
                    gore.velocity += Vector2.Normalize(Projectile.Center - gore.position) * gravityMagnitude;
                }
            }
        }
        private void SuckDust(float suckingRange, float suckingStrength = 20)
        {
            foreach (Dust dust in Main.dust)
            {
                if (!dust.active)
                {
                    continue;
                }

                float dist = Vector2.Distance(Projectile.Center, dust.position);
                if (dist <= suckingRange)
                {
                    float gravityMagnitude = Projectile.scale * suckingStrength / (dist * 0.5f + 10f); //Won't divide by 0 :)
                    dust.velocity += Vector2.Normalize(Projectile.Center - dust.position) * gravityMagnitude;
                }
            }
        }
        private void Explode()
        {
            //Change hitbox size and damage
            Projectile.ExpandHitboxTo((int)(Projectile.width * 1.5f), (int)(Projectile.height * 1.5f));
            int explosionDamage = Projectile.damage + Projectile.width;

            foreach (NPC npc in Main.npc)
            {
                //Find any valid npc inside the hitbox
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    //Deal the damage, ignoring their iframes
                    var info = npc.CalculateHitInfo(explosionDamage, Projectile.direction);
                    npc.StrikeNPC(info);
                    NetMessage.SendStrikeNPC(npc, info);
                }
            }

            Projectile.Kill();
        }
        private void HandleAudioLoop()
        {
            if (Main.GameUpdateCount % 40 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.95f }, Projectile.position);
            }
        }
    }
}
