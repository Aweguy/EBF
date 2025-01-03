using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic.Flameheart
{
    public class Flameheart : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        int ChargeStacks = 0;
        public override void SetDefaults()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 36;//Item's base damage value
            Item.knockBack = 2f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 16;//The amount of mana this item consumes on use
            Item.DamageType = DamageClass.Magic;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 20;//How fast the item is used
            Item.useAnimation = 20;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 75, gold: 2, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item20;//The item's sound when it's used
            Item.autoReuse = false;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<Flameheart_Fireball>();
            Item.noMelee = true;//Prevents damage from being dealt by the item itself
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (++ChargeStacks >= 3)
            {
                type = ModContent.ProjectileType<Flameheart_Firestorm>();
                ChargeStacks = 0;
            }
            position = Main.MouseWorld;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, stack: 20)
                .AddIngredient(ItemID.SoulofLight, stack: 15)
                .AddIngredient(ItemID.Fireblossom, stack: 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class Flameheart_Firestorm : ModProjectile
    {
        private int timer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;

            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.knockBack = 1f;

            Projectile.timeLeft = 51;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(10))
            {
                target.AddBuff(BuffID.OnFire, 300, false);
            }
        }

        public override void AI()
        {
            Projectile.damage = 0;

            timer--;

            if (timer <= 0)
            {
                //Randomize projectile
                int chosenProjectile = 0;
                switch (Main.rand.Next(3))
                {
                    case 0:
                        chosenProjectile = ModContent.ProjectileType<Flameheart_FireballSmall>();
                        break;
                    case 1:
                        chosenProjectile = ModContent.ProjectileType<Flameheart_FireballMed>();
                        break;
                    case 2:
                        chosenProjectile = ModContent.ProjectileType<Flameheart_Fireball>();
                        break;
                }

                //Spawn projectile
                float X = Projectile.Center.X + Main.rand.NextFloat(-100f, 100f);
                float Y = Projectile.Center.Y + Main.rand.NextFloat(-100f, 100f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), X, Y, 0f, 0f, chosenProjectile, 70, 0, Projectile.owner);
                timer = 5;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return true;
        }
    }

    public abstract class Flameheart_FireballBase : ModProjectile
    {
        /// <summary>
        /// Sets the variables that are shared between all fireball sizes.
        /// <para>If one of these variables should differ between fireballs, then move the variable into each subclass.</para>
        /// </summary>
        protected void SetEverythingElse()
        {
            Projectile.aiStyle = -1;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.knockBack = 1f;

            Projectile.timeLeft = 100;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.OnFire, 300, false);
            }
        }
        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                // You need to set position depending on what you are doing. You may need to subtract width/2 and height/2 as well to center the spawn rectangle.
                Vector2 position = Projectile.position;
                Dust.NewDustDirect(position, Projectile.width, Projectile.height, DustID.Pixie, 0.2631578f, -2.368421f, 0, new Color(255, 251, 0), 1.25f);
            }

            if (++Projectile.frameCounter > 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 14)
                {
                    Projectile.Kill();
                }
            }
        }
    }

    public class Flameheart_Fireball : Flameheart_FireballBase
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 13;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            SetEverythingElse();   
        }
    }

    public class Flameheart_FireballMed : Flameheart_FireballBase
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 13;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            SetEverythingElse();
        }
    }

    public class Flameheart_FireballSmall : Flameheart_FireballBase
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 14;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            SetEverythingElse();
        }
    }
}
