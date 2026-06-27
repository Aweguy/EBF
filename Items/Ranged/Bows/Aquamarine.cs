using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class Aquamarine : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<Aquamarine_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 28;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 70;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 42;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 10, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 7f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Coral, stack: 16)
                .AddIngredient(ItemID.Sapphire, stack: 10)
                .AddIngredient(ItemID.SoulofLight, stack: 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class Aquamarine_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<Aquamarine_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/Aquamarine";
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 70;
            MaximumDrawTime = 50;
            DamageScale = 1.25f;
            VelocityScale = 1.75f;
            base.SetDefaults();
        }
    }

    public class Aquamarine_Arrow : ModProjectile
    {
        private float baseSpeed;
        private bool inBubble = false;
        private bool fullyCharged = false;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = 5;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.friendly = true;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if ((int)Projectile.ai[0] == 1)
                fullyCharged = true; // Stored as separate field in OnSpawn because aiStyle overwrites ai
            
            baseSpeed = Projectile.velocity.Length();
        }

        public override void AI()
        {
            if (inBubble)
            {
                Projectile.localAI[2]--;
                if (Projectile.localAI[2] <= 0)
                {
                    Projectile.aiStyle = ProjAIStyleID.Arrow;
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * baseSpeed;
                    inBubble = false;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (fullyCharged)
            {
                Projectile.aiStyle = 0;
                Projectile.velocity = Vector2.Normalize(Projectile.velocity);

                var pos = Projectile.Center + Projectile.velocity * 8;
                var type = ModContent.ProjectileType<Aquamarine_Bubble>();
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Vector2.Zero, type, Projectile.damage, Projectile.knockBack);

                inBubble = true;
                Projectile.localAI[2] = 30;
            }
        }
    }

    public class Aquamarine_Bubble : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Gore_415";
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item85, Projectile.position);
            Projectile.scale = 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Wet, 60 * 5);
        }
        public override void AI()
        {
            Projectile.scale = Math.Min(1f, Projectile.scale + (1f / 30f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false; // skip default draw
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.friendly = true;
            Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item54, Projectile.position);

            for (int i = 0; i < 20; i++)
            {
                var speedX = Main.rand.NextFloat(-4, 4);
                var speedY = Main.rand.NextFloat(-4, 4);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, speedX, speedY);
            }
        }
    }
}
