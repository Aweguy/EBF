using EBF.EbfUtils;
using EBF.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee.Throwable
{
    public class Stormsplitter : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 64;
            Item.damage = 168;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing; // Throwing style
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Stormsplitter_Proj>();
            Item.shootSpeed = 14f;
            Item.value = Item.sellPrice(platinum: 0, gold: 25, silver: 0, copper: 0);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.DayBreak, stack: 1)
                .AddIngredient<HolyGrail>(stack: 1)
                .AddIngredient<ElixirOfLife>(stack: 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class Stormsplitter_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.aiStyle = 1;
            AIType = ProjectileID.JavelinFriendly;
            Projectile.extraUpdates = 1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            foreach (var proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is Stormsplitter_Thunderball thunderball && thunderball.Target == null)
                {
                    thunderball.AssignTarget(target);
                }
            }
        }
        public override void AI()
        {
            if (Projectile.numUpdates == 0 && Main.GameUpdateCount % 10 == 0)
            {
                var type = ModContent.ProjectileType<Stormsplitter_Thunderball>();
                var damage = (int)(Projectile.damage * 0.75f);
                var perpendicularVelocity = Vector2.Normalize(new Vector2(-Projectile.velocity.Y, Projectile.velocity.X)) * 5f;
                var backVelocity = Vector2.Normalize(-Projectile.velocity) * 4f;

                var thunderball = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, perpendicularVelocity + backVelocity, type, damage, 0, Projectile.owner, ai0: Projectile.whoAmI);
                thunderball.netUpdate = true;

                thunderball = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, -perpendicularVelocity + backVelocity, type, damage, 0, Projectile.owner, ai0: Projectile.whoAmI);
                thunderball.netUpdate = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Draws an afterimage trail. See https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#afterimage-trail for more information.

            var texture = TextureAssets.Projectile[Type].Value;
            var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                var color = Color.Orange * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Projectile.Resize(32, 32);
            Projectile.CreateExplosionEffect();
            Projectile.Damage();
        }
    }

    public class Stormsplitter_Thunderball : ModProjectile
    {
        public NPC Target;
        private Vector2 startPos;
        private float timer;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (Target != null && Target.active)
            {
                //Attract to target
                timer++;
                float duration = 30f;
                float t = MathHelper.Clamp(timer / duration, 0f, 1f);
                float eased = t * t * ((1.70158f + 1f) * t - 1.70158f); // Ease In Back
                Projectile.Center = Vector2.Lerp(startPos, Target.Center, eased);

                //Spawn dust
                var delta = Projectile.oldPosition - Projectile.position;
                var amount = Math.Clamp((int)delta.Length(), 0, 6);
                for (int i = 0; i < amount; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.OrangeTorch, Vector2.Zero, Scale: 1f);
                    dust.position -= delta / amount * i;
                    dust.noGravity = true;
                }
            }
            else
            {
                Projectile.velocity *= 0.95f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item118, Projectile.position);
            Projectile.Resize(30, 30);
            Projectile.CreateExplosionEffect(EBFUtils.ExplosionSize.Small);
            Projectile.Damage();
        }
        public void AssignTarget(NPC npc)
        {
            Target = npc;
            startPos = Projectile.Center;
            timer = 0f;
        }
    }
}
