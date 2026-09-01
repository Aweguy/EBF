using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public abstract class FanWeapon : EBFStaff, ILocalizedModType
    {
        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 8;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 6;//The amount of mana this item consumes on use

            Item.useTime = 4;//How fast the item is used
            Item.useAnimation = 4;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 1, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<PaperFan_Gale>();
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 1; i++)
                Projectile.NewProjectile(source, position, velocity + Main.rand.NextVector2Square(-2f, 2f), type, damage, knockback);

            return false;
        }

        public class PaperFan_Gale : ModProjectile
        {
            public override void SetDefaults()
            {
                Projectile.width = 20;
                Projectile.height = 20;
                Projectile.friendly = true;
                Projectile.DamageType = DamageClass.Magic;
                Projectile.tileCollide = true;
                Projectile.timeLeft = 20;
                Projectile.velocity.RotatedByRandom(0.2d);
                Projectile.alpha = 0;
            }
            public override void SetStaticDefaults()
            {
                ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10; // The length of old position to be recorded
                ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
            }
            public override bool PreDraw(ref Color lightColor)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
                for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
                {
                    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                }

                return true;
            }
            public override string Texture => "EBF/Items/Magic/Gale";
            public override void AI()
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // projectile sprite faces up

                Projectile.ai[0] += 1f;

                FadeOut();
                {
                    Projectile.velocity.X = Projectile.velocity.X * 0.92f;
                    Projectile.velocity.Y = Projectile.velocity.Y * 0.92f;
                    
                }
            }
            public void FadeOut()
            {
                // If last less than 12 ticks — fade in, than more — fade out
                if (Projectile.ai[0] <= 12f)
                    // Fade out
                    Projectile.alpha += 19;

            }
        }
    }
    public class PaperFan : FanWeapon, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
            .AddIngredient(ItemID.Cobweb, stack: 20)
            .AddIngredient(ItemID.Wood, stack: 10)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }
}