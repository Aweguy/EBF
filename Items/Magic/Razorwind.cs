using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace EBF.Items.Magic
{
    public class Razorwind : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 158;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 9;//The amount of mana this item consumes on use

            Item.useTime = 60;//How fast the item is used
            Item.useAnimation = 60;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 80, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
            Item.channel = true;

            Item.shoot = ModContent.ProjectileType<Razorwind_Projectile>();
            Item.shootSpeed = 18f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = (Main.MouseWorld - StaffHead) * 0.055f;
            velocity.Y -= 20f;

            //Spawn the projecile
            SoundEngine.PlaySound(SoundID.Item9, player.Center);
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<ArcticTrident>(stack: 1)
                .AddIngredient(ItemID.LunarBar, stack: 10)
                .AddIngredient(ItemID.Ectoplasm, stack: 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class Razorwind_Projectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontCancelChannelOnKill[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The trail recording mode
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ice);
            }
        }
        public override void AI()
        {
            //Move down for a bit
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] < 30)
            {
                Projectile.velocity.Y += 0.75f;
            }

            //Slow down and rotate based on velocity
            Projectile.velocity *= 0.95f;
            Projectile.rotation = Projectile.velocity.X / 2;
            
            //Spawn dust
            if (Main.rand.NextBool(5))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0, 2);
            }

            //Handle mouse release
            Player player = Main.player[Projectile.owner];
            if (!player.channel)
            {
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Draws an afterimage trail. See https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#afterimage-trail for more information.

            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.Blue * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            //Spawn dust
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ice);
            }

            //Spawn a ring of icecicles
            int projectileCount = 6;
            for (float angle = 0; angle < Math.Tau - 0.01f; angle += (float)Math.Tau / projectileCount)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    angle.ToRotationVector2() * 5,
                    ModContent.ProjectileType<ArcticTrident_Icecicle>(),
                    Projectile.damage,
                    Projectile.knockBack / 2,
                    Projectile.owner);
            }
        }
    }
}
