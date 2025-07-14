using EBF.EbfUtils;
﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace EBF.Items.Magic
{
    public class DragonWings : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 135;//Item's base damage value
            Item.knockBack = 5;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 15;//The amount of mana this item consumes on use

            Item.useTime = 5;//How fast the item is used
            Item.useAnimation = 40;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
            
            Item.shoot = ModContent.ProjectileType<DragonWings_Projectile>();
            Item.shootSpeed = 8f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Quirky way to shoot 3 times
            if(player.itemAnimation <= 30)
            {
                player.itemTime = 30;
            }

            //Randomize velocity slightly
            velocity -= Vector2.UnitY * Main.rand.NextFloat(2, 4);

            //Spawn the projecile
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, player.Center);
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<Flameheart.Flameheart>(stack: 1)
                .AddIngredient(ItemID.FragmentSolar, stack: 15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class DragonWings_Projectile : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2BetsyFireball}";
        
        private NPC target = null;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 0.75f;
            Projectile.timeLeft = 600;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 60 * 5);
        }
        public override bool PreAI()
        {
            SetTarget();

            //If there's a valid target, home towards it
            if(target != null)
            {
                Projectile.HomeTowards(target, maxSpeed: 12, strength: 2);

                //The first time the homing activates:
                if (Projectile.localAI[1] == 0)
                {
                    Projectile.localAI[1] = 1;

                    SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot, Projectile.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Scale: 1.25f);
                    }
                }
            }
            else
            {
                //Otherwise, use gravity
                Projectile.velocity += Vector2.UnitY * 0.2f;
            }

            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch); 
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Draws an afterimage trail. See https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#afterimage-trail for more information.

            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.OrangeRed * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Projectile.Center);
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Main.rand.Next(61, 64));
        }
        private void SetTarget()
        {
            Projectile.localAI[0]++;

            //Limit how often we search for targets for performance
            //And delay beginning check for extra flair
            if (Projectile.localAI[0] > 50)
            {
                Projectile.localAI[0] = 30;
                if(EBFUtils.ClosestNPC(ref target, 400, Projectile.position))
                {
                    return;
                }
                else
                {
                    target = null;
                }
            }
        }
    }
}
