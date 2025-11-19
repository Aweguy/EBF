using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Summon
{
    public class BusterSword : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 36;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 48;//Item's base damage value
            Item.knockBack = 4f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 2, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 4;

            Item.shoot = ModContent.ProjectileType<BusterSwordStab>();
            BonusMinion = ModContent.ProjectileType<OrigamiDragonMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 4;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.BambooBlock, stack: 200)
                .AddIngredient(ItemID.TatteredCloth, stack: 3)
                .AddIngredient(ItemID.SoulofMight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class BusterSwordStab : EBFToyStab
    {
        public override void SetDefaultsSafe()
        {
            DrawOffsetX = -4;
            DrawOriginOffsetY = -6;

            ProjOffset = 8;
            BoostDuration = 60;
            TagDamage = 5;
        }
    }

    public class OrigamiDragonMinion : EBFMinion
    {
        private int[] idleAnimSequence = { 0, 1, 2, 1 };
        private int[] attackAnimSequence = { 0, 3, 4, 3 };
        private const int paperBladeOffset = 150;
        private const float maxSpeed = 10;
        public override string Texture => "EBF/Items/Summon/BusterSword_OrigamiDragonMinion";
        public override bool MinionContactDamage() => true;
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 74;
            Projectile.height = 74;
            Projectile.tileCollide = true;
            UseHoverAI = true;
            AttackRange = 80;
            AttackTime = 4;
            MoveSpeed = 30;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Zombie27, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
            }
        }
        public override void AISafe()
        {
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }

            Projectile.friendly = Target != null;
            Animate();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0] = 1;
            Projectile.frameCounter = 0;
        }
        public override void OnAttack(NPC target)
        {
            if (IsBoosted)
            {
                BoostTime = 0;

                //Spawn paper blades
                int type = ModContent.ProjectileType<PaperBladeProjectile>();
                for (int i = 0; i < 5; i++)
                {
                    Vector2 position = target.Center + Projectile.DirectionFrom(target.Center).RotatedByRandom(1.25f) * paperBladeOffset;
                    Vector2 velocity = position.DirectionTo(target.Center) * 0.05f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, type, Projectile.damage * 2, Projectile.knockBack, Projectile.owner);
                }
            }
        }
        private void Animate()
        {
            //Funny guard clause to make idle animation play slower than attack animation 
            if (Main.GameUpdateCount % 4 != 0 && Projectile.ai[0] == 0)
                return;

            if (Main.GameUpdateCount % 2 == 0)
            {
                //Determine frame sequence index
                Projectile.frameCounter++;
                if (Projectile.frameCounter > 3)
                {
                    Projectile.frameCounter = 0;
                    Projectile.ai[0] = 0; //Use idle anim
                }

                if (Projectile.ai[0] == 1)
                {
                    Projectile.frame = attackAnimSequence[Projectile.frameCounter];
                }
                else
                {
                    Projectile.frame = idleAnimSequence[Projectile.frameCounter];
                }
            }
        }
    }
    public class PaperBladeProjectile : ModProjectile
    {
        private float rotationOffset; //Lerps to 0 before blade accelerates
        public override string Texture => "EBF/Items/Summon/BusterSword_PaperBladeProjectile";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            DrawOriginOffsetY = -30;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            rotationOffset = Projectile.ai[0] + (float)Math.Tau + MathHelper.PiOver2;
        }
        public override void AI()
        {
            if (Projectile.frameCounter >= 20)
            {
                //Accelerate to max speed
                Projectile.velocity *= 1.25f;
                if (Projectile.velocity.Length() > 40)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 40;

                    SpawnDustTrail();
                }
            }
            else
            {
                //Rotate around origin
                Projectile.frameCounter++;
                rotationOffset = float.Lerp(rotationOffset, Projectile.ai[0], 0.25f);
                Projectile.rotation = rotationOffset;
            }
        }
        public override void OnKill(int timeLeft)
        {
            Vector2 v = Projectile.velocity * 0.1f;
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SandSpray, v.X, v.Y);
            }
        }
        private void SpawnDustTrail()
        {
            int n = (int)Projectile.velocity.Length();
            var v = Vector2.Normalize(Projectile.velocity);
            for (int i = 0; i < n; i++)
            {

                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch);
                d.noGravity = true;
                d.position += v * i;

            }

        }
    }
}
