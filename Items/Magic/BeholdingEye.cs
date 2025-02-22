using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class BeholdingEye : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 28;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 5;//The amount of mana this item consumes on use

            Item.useTime = 8;//How fast the item is used
            Item.useAnimation = 24;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.UseSound = SoundID.Item103;
            Item.shoot = ModContent.ProjectileType<BeholdingEye_Projectile>();
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Use staff head
            position = StaffHead;
            velocity = StaffHead.DirectionTo(Main.MouseWorld).RotatedByRandom(0.25f) * Item.shootSpeed;

            //Create required ai for tentacle rotation
            float tentacleYDirection = Main.rand.NextFloat(-0.08f, 0.08f);
            float tentacleXDirection = Main.rand.NextFloat(-0.08f, 0.08f);

            //Spawn the projectile
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BeholdingEye_Projectile>(), damage, knockback, Main.myPlayer, tentacleXDirection, tentacleYDirection);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Coral, stack: 20)
                .AddIngredient(ItemID.Sapphire, stack: 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class BeholdingEye_Projectile : ModProjectile
    {
        private Vector2 spawnedPosition; //Used to move dust backwards after it has spawned
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 2;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            spawnedPosition = Projectile.Center;
        }
        public override void AI() //Based on Calamity Mod's Eldritch Tome
        {
            if (Projectile.velocity.HasNaNs())
            {
                Projectile.Kill();
                return;
            }

            //Adjust size and correct position
            Projectile.scale = 1f - Projectile.localAI[0];
            Projectile.width = (int)(Projectile.scale * 20);
            Projectile.height = Projectile.width;
            Projectile.Center = Projectile.Center;

            //Increase size reduction
            Projectile.localAI[0] += Projectile.localAI[0] < 0.1f ? 0.01f : 0.025f;
            if (Projectile.localAI[0] >= 0.95f)
            {
                Projectile.Kill();
                return;
            }

            //Apply tentacle curve and limit velocity
            Projectile.velocity += new Vector2(Projectile.ai[0], Projectile.ai[1]) * 1.5f;
            if (Projectile.velocity.Length() > 16f)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= 16f;
            }

            //Increase tentacle curve
            Projectile.ai[0] *= 1.05f;
            Projectile.ai[1] *= 1.05f;
            
            //Spawn dusts
            if (Projectile.scale <= 1f)
            {
                for (float dustScale = 0; dustScale < Projectile.scale; dustScale += 0.15f)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Crimson, 0, 0, Scale: 1 + Projectile.scale);
                    dust.velocity = (spawnedPosition - Projectile.Center) / 32;
                    dust.fadeIn = Projectile.scale * 2; //make small dusts die faster
                    dust.noGravity = true;
                }
            }
        }
    }
}
