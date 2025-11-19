using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class OakStaff : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 16;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 7;//The amount of mana this item consumes on use

            Item.useTime = 22;//How fast the item is used
            Item.useAnimation = 22;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 80, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.UseSound = SoundID.Item127;
            Item.shoot = ModContent.ProjectileType<OakStaff_Idol>();
            Item.shootSpeed = 7f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = StaffHead.DirectionTo(Main.MouseWorld).RotatedByRandom(0.2f) * Item.shootSpeed;
            velocity -= Vector2.UnitY * 2;

            //Sometimes shoot a roller instead
            if (Main.rand.NextBool(5))
            {
                type = ModContent.ProjectileType<OakStaff_RollerProjectile>();
            }

            //Spawn the projecile
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI);

            //Spawn some dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(StaffHead, 0, 0, DustID.WoodFurniture);
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Wood, stack: 120)
                .AddIngredient(ItemID.JungleSpores, stack: 10)
                .AddTile(TileID.LivingLoom)
                .Register();
        }
    }

    public class OakStaff_Idol : ModProjectile
    {
        private int bounces = 6;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(3);
            Projectile.scale = 0.1f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounces--;
            if (bounces == 0)
            {
                Projectile.Kill();
                return false;
            }

            //Bounce vertical
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.8f;
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
                }
            }

            //Bounce horizontal
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.8f;
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
                }
            }

            return false;
        }
        public override void AI()
        {
            if (Projectile.scale < 1)
            {
                Projectile.scale += 0.1f;
            }

            Projectile.velocity.Y += 0.2f;
            Projectile.rotation += Projectile.velocity.X * 0.025f;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            //Spawn dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.WoodFurniture);
            }
        }
    }

    public class OakStaff_RollerProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0.1f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
            }

            //Bounce
            if (oldVelocity.Y > 1)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
                }

                Projectile.velocity.Y = -oldVelocity.Y * 0.33f;
            }

            //Handle block climbing
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Vector2 wallPosition = Projectile.Center + new Vector2(27 * Projectile.direction, -16);
                Tile tile = Framing.GetTileSafely(wallPosition);

                //Check if the new position is inside a block
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    Projectile.Kill();
                }
                else
                {
                    //Keep going
                    Projectile.position -= Vector2.UnitY * 16;
                    Projectile.velocity = oldVelocity;
                    Projectile.velocity.Y = 0;
                }
            }
            return false;
        }
        public override void AI()
        {
            //Scale in
            if (Projectile.scale < 1)
            {
                Projectile.scale += 0.1f;
            }

            //Gravity & animation
            Projectile.velocity.Y += 0.33f;
            if (Main.GameUpdateCount % 8 == 0)
            {
                Projectile.frame = ++Projectile.frame % 2;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Get ground below target
            Vector2 position = target.Center.ToGroundPosition();

            //Spawn projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<OakStaff_LogSpell>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public class OakStaff_LogSpell : ModProjectile
    {
        private int[] frameSequence = { 0, 1, 2, 3, 4, 3, 2, 1, 0 };
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.position.Y -= Projectile.height / 2;

            for (int i = 0; i < 10; i++)
            {
                //Spawn dirt dust
                Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(-2, 3) + Projectile.height), DustID.Dirt, Vector2.Zero, 0, default, 3f);
                dust.noGravity = true;
            }
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 4 == 0)
            {
                //Handle animation
                Projectile.frameCounter++;
                if (Projectile.frameCounter > 8)
                {
                    Projectile.Kill();
                }
                else
                {
                    Projectile.frame = frameSequence[Projectile.frameCounter];
                }
            }
        }
    }
}
