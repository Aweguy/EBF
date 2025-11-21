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
    public class Aquamarine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 26;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 46;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 42;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 10, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item32;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 7f;
            Item.channel = true;
            Item.noMelee = true;
        }
        public override bool CanUseItem(Player player) => player.HasAmmo(player.HeldItem) && !player.noItems && !player.CCed;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
                type = ModContent.ProjectileType<Aquamarine_Arrow>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Coral, stack: 16)
                .AddIngredient(ItemID.Sapphire, stack: 10)
                .AddIngredient(ItemID.SoulofLight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class Aquamarine_Arrow : EBFChargeableArrow
    {
        private float baseSpeed;
        private bool inBubble = false;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = 2;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            MinimumDrawTime = 10;
            DamageScale = 1.25f;
            VelocityScale = 1.75f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void AI()
        {
            // Run only once
            if (Projectile.localAI[1]++ == 0)
                baseSpeed = Projectile.velocity.Length();

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
        public override void OnProjectileRelease()
        {
            if (FullyCharged)
            {
                Projectile.penetrate = 5;
                Projectile.stopsDealingDamageAfterPenetrateHits = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (FullyCharged)
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
