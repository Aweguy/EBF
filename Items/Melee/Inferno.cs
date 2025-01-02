using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class Inferno : ModItem
    {

        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Inferno");//Name of the Item
            base.Tooltip.WithFormatArgs("Wreathed in scorching flames.\nBurns foes.");//Tooltip of the item
        }

        public override void SetDefaults()
        {
            Item.width = 60;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 60;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 20;//Item's base damage value
            Item.knockBack = 1f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Rapier;//The animation of the item when used
            Item.useTime = 5;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Cyan;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.noUseGraphic = true; // Important, because otherwise you'd sometimes see a duplicate item sprite
            Item.shoot = ModContent.ProjectileType<Inferno_Proj>();
            Item.shootSpeed = 2;
        }
    }

    /* TODO: Use a vertical sprite instead of rotating the diagonal sprite using PreDraw().
     * This is needed because the hitbox currently doesn't match the projectile.
     */

    /// <summary>
    /// Shortswords act as projectiles. So this class represents the blade that is seen upon using the Inferno item.
    /// </summary>
    public class Inferno_Proj : ModProjectile
    {
        private const float positionOffset = 25f;
        private float rotationOffset;
        public override string Texture => "EBF/Items/Melee/Inferno";
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = 1.2f;
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            DrawOriginOffsetX = -10;
            DrawOriginOffsetY = -10;

            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            rotationOffset = Main.rand.NextFloat(-0.33f, 0.33f);
            Projectile.velocity = Projectile.velocity.RotatedBy(rotationOffset);
            Projectile.rotation += rotationOffset;
        }
        public override void PostAI() //Post because otherwise the shortsword aiStyle would override any changes
        {
            Projectile.position += Vector2.Normalize(Projectile.velocity) * positionOffset;
        }
        public override bool PreDraw(ref Color lightColor) //This is used here to correct the sprite rotation
        {
            // Get the texture of the projectile
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Source rectangle for animation frames (if needed)
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);

            // The origin of the sprite, typically its center
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // Adjust the rotation to account for the diagonal texture
            float correctedRotation = Projectile.rotation - MathHelper.PiOver4; // Rotate sprite

            // Draw the projectile, applying the corrected rotation
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor, correctedRotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, 360)) * 4;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, velocity, ModContent.ProjectileType<Inferno_Fireball>(), Projectile.damage / 2, KnockBack: 0);
            }

            target.AddBuff(BuffID.OnFire, 60 * 2);
        }
    }

    public class Inferno_Fireball : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 0.5f;

            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 60 * 2;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 60 * 2);
        }

        public override bool PreAI()
        {
            Projectile.velocity *= 0.95f;

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, Color.Orange, 1f);
                dust.noGravity = true;
            }

            return false;
        }
    }
}
