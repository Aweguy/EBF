using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class CrystalStaff : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        private const int Spread = 48;
        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 26;//Item's base damage value
            Item.knockBack = 5;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 3;//The amount of mana this item consumes on use

            Item.useTime = 24;//How fast the item is used
            Item.useAnimation = 24;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item43;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<CrystalStaff_Projectile>();
            Item.shootSpeed = 0.2f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Spawn position
            var offset = new Vector2(16 * player.direction, -256 * player.gravDir);
            position = StaffHead + offset + Main.rand.NextVector2CircularEdge(Spread, Spread);

            //Velocity towards cursor
            velocity = Vector2.Normalize(Main.MouseWorld - position) * velocity.Length();

            //Spawn the projecile
            damage += Main.rand.Next(-20, 21);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        // Sold by Natalie
    }

    public class CrystalStaff_Projectile : ModProjectile
    {
        private const int DustsOnDeath = 10;
        private const int DustsOnBounce = 20;
        private const int MaxVelocity = 20;
        private const float ShrinkSpeed = 0.02f; // scale per frame
        private const float GrowSpeed = 0.02f; // scale per frame
        private bool isShrinking = false;
        public override string Texture => "EBF/Items/Magic/Star";
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Bounce(Projectile.oldVelocity);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.tileCollide)
            {
                Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
                if (isShrinking)
                {
                    return true;
                }

                Bounce(oldVelocity);
            }

            return false;
        }
        public override void AI()
        {
            var vel = Projectile.velocity;
            var velLength = vel.Length();

            //Handle shrinking & despawning
            if (isShrinking)
            {
                vel.X = MathHelper.Clamp(vel.X * 0.9f, -MaxVelocity, MaxVelocity);
                vel.Y = MathHelper.Clamp(vel.Y * 0.9f, -MaxVelocity, 0);
                Projectile.scale -= ShrinkSpeed;
                if (Projectile.scale <= 0)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                vel *= 1.1f;

                if (velLength > 2)
                {
                    SpawnDusts(1);
                    if (velLength > MaxVelocity)
                    {
                        vel = Vector2.Normalize(vel) * MaxVelocity;
                    }
                }

                if (Projectile.scale < 1)
                {
                    Projectile.scale += GrowSpeed;
                }
                else
                {
                    Projectile.tileCollide = true;
                }
            }

            Projectile.rotation += vel.Length() * 0.1f;
            Projectile.velocity = vel;
        }
        public override void OnKill(int timeLeft)
        {
            SpawnDusts(DustsOnDeath);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
        private void SpawnDusts(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            }
        }
        private void Bounce(Vector2 oldVelocity)
        {
            isShrinking = true;
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            //Bounce
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.5f;
            }

            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
                Projectile.velocity.X *= 0.5f;
            }

            SpawnDusts(DustsOnBounce);
        }
    }
}
