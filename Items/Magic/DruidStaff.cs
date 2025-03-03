using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace EBF.Items.Magic
{
    public class DruidStaff : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 18;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 6;//The amount of mana this item consumes on use

            Item.useTime = 18;//How fast the item is used
            Item.useAnimation = 18;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 80, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.UseSound = SoundID.Item17;
            Item.shoot = ModContent.ProjectileType<DruidStaff_Projectile>();
            Item.shootSpeed = 6f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = StaffHead.DirectionTo(Main.MouseWorld) * Item.shootSpeed;
            velocity.Y -= 4f;

            //Spawn the projecile
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.JungleSpores, stack: 10)
                .AddIngredient(ItemID.Vine, stack: 5)
                .AddIngredient(ItemID.Stinger, stack: 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class DruidStaff_Projectile : ModProjectile
    {
        private bool flip; //Used to determine thorn side
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            //Handle gravity
            Tile tile = Framing.GetTileSafely(Projectile.Center);

            //Check if the new position is inside a block
            if (tile.HasTile && Main.tileSolid[tile.TileType])
            {
                Projectile.velocity.Y *= 0.95f;
                Projectile.velocity.Y -= 0.3f;
            }
            else
            {
                Projectile.velocity.Y += 0.15f;
            }

            //Spawn dust
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, Vector2.Zero, Scale: 2);
            dust.noGravity = true;
            dust.noLight = true;

            //Spawn thorns occasionally
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 16)
            {
                Projectile.frameCounter = 0;

                Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2 * (flip == true ? -1 : 1));
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Normalize(velocity), ModContent.ProjectileType<DruidStaff_Thorn>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                proj.position += proj.velocity * 4;
                proj.rotation = proj.velocity.ToRotation() + MathHelper.PiOver2;
                proj.velocity *= 0.2f;

                flip = !flip;
            }
        }
    }
    public class DruidStaff_Thorn : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.scale -= 0.025f;
            if(Projectile.scale <= 0)
            {
                Projectile.Kill();
            }
        }
    }
}
