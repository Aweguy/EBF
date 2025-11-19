using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Summon
{
    public class PowerPaw : EBFCatToy, ILocalizedModType
    {
        private int projAnimAI = 0; //Allows us to alternate the animation the punch projectile uses.
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 36;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 72;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 16;//How fast the item is used
            Item.useAnimation = 16;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 40, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Lime;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 4;

            Item.shoot = ModContent.ProjectileType<PowerPawPunch>();
            BonusMinion = ModContent.ProjectileType<GunslingerMinion>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, projAnimAI);
            projAnimAI = projAnimAI == 0 ? 1 : 0;
            return false;
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 6;
        }
        //Bought by Arms Dealer after defeating plantera
    }

    public class PowerPawPunch : EBFToyStab
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 12;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 64;
            Projectile.height = 64;

            ProjOffset = 4;
            BoostDuration = 180;
            TagDamage = 5;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Projectile.ai[0] == 0 ? 0 : 6;
            Projectile.LookAt(Main.MouseWorld);
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 3 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 6 && Projectile.ai[0] == 0)
                {
                    Projectile.Kill();
                }
                else if (Projectile.frame >= 12)
                {
                    Projectile.Kill();
                }
            }
        }
    }

    public class GunslingerMinion : EBFMinion
    {
        private Texture2D gunTexture;
        public override string Texture => "EBF/Items/Summon/PowerPaw_GunSlingerMinion";
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 8;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 56;
            Projectile.height = 78;
            Projectile.tileCollide = false;
            UseHoverAI = true;
            DetectRange = 700;
            AttackRange = 600;
            AttackTime = 20;
            MoveSpeed = 4f;
            gunTexture = ModContent.Request<Texture2D>("EBF/Items/Summon/PowerPaw_GunSlingerGun").Value;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item113, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand);
            }
        }
        public override void AISafe()
        {
            Animate();
        }
        public override void PostDraw(Color lightColor)
        {
            if (InAttackRange)
            {
                var sourceRect = gunTexture.Bounds;
                var vectorToTarget = Projectile.DirectionTo(Target.Center);
                var drawPos = (Projectile.Center - Main.screenPosition) + (vectorToTarget * 20) + (Vector2.UnitY * 8);
                var rotation = vectorToTarget.ToRotation();
                var spriteFX = vectorToTarget.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
                Main.EntitySpriteDraw(gunTexture, drawPos, sourceRect, Color.White, rotation, gunTexture.Size() / 2, 1, spriteFX);
            }
        }
        public override void OnAttack(NPC target)
        {
            int type = ProjectileID.BulletHighVelocity;
            Vector2 velocity = Projectile.DirectionTo(target.Center) * 8;
            var sfx = SoundID.Item11;

            if (IsBoosted)
            {
                type = ProjectileID.RocketI;
                velocity *= 2;
                sfx = SoundID.Item99;
            }

            SoundEngine.PlaySound(sfx, Projectile.position);
            var p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
            p.friendly = true;
        }
        private void Animate()
        {
            if (Main.GameUpdateCount % 6 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 8)
                {
                    Projectile.frame = 0;
                }
            }
        }
    }
}
