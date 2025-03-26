using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.GameContent.Drawing;
using Terraria.DataStructures;

namespace EBF.Items.Summon
{
    public class IcecreamSandwich : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 42;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 14;//Item's base damage value
            Item.knockBack = 13f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 30, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 2;

            Item.shoot = ModContent.ProjectileType<IcecreamSandwichStab>();
            BonusMinion = ModContent.ProjectileType<IcecreamSlime>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 2;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.SnowBlock, stack: 99)
                .AddIngredient(ItemID.Shiverthorn, stack: 3)
                .AddIngredient(ItemID.Cherry, stack: 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class IcecreamSandwichStab : ModProjectile
    {
        private const int projOffset = 6;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            DrawOffsetX = 0;
            DrawOriginOffsetY = -6;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Item item = Main.player[Projectile.owner].HeldItem;
            if (item.ModItem is EBFCatToy toy)
            {
                toy.ApplyBoost(180);
                target.AddBuff(BuffID.Frostburn, 360);

                //Spawn fancy hit particle
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center });
            }
        }
        public override void PostAI()
        {
            Projectile.position += Projectile.velocity * projOffset;
        }
    }

    public class IcecreamSlime : EBFMinion
    {
        public override string Texture => "EBF/Items/Summon/IcecreamSandwich_IcecreamSlimeMinion";
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 44;
            Projectile.height = 48;
            AttackRange = 350;
            AttackTime = 40;
            MoveSpeed = 5f;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item154, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud);
            }
        }
        public override void OnAttack(NPC target)
        {
            Projectile.frame = 2;
            Projectile.frameCounter = 1;

            var shootSound = SoundID.Item5;
            var velocity = Projectile.Center.DirectionTo(target.Top) * 8;
            var type = ModContent.ProjectileType<IcecreamSandwich_WaferProjectile>();
            var damage = Projectile.damage;
            
            if (IsBoosted)
            {
                velocity *= 2;
                damage *= 4;
                shootSound = SoundID.Item42;

                BoostedEffect();
            }

            SoundEngine.PlaySound(shootSound, Projectile.Center);
            var p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, type, damage, Projectile.knockBack, Projectile.owner);
            if (IsBoosted)
            {
                p.penetrate = -1;
            }
        }
        public override void AISafe()
        {
            if (Math.Abs(Projectile.velocity.X) > 2f && !InAttackRange)
            {
                JumpTo(Projectile.Center - Vector2.UnitY * 8);
            }

            Animate();
        }
        private void Animate()
        {
            if (Main.GameUpdateCount % 6 == 0)
            {
                Projectile.frame++;

                //Idle animation
                if(Projectile.frameCounter == 0 && Projectile.frame > 1)
                {
                    Projectile.frame = 0;
                }

                //Attacking animation, initial frame set in OnAttack()
                if(Projectile.frameCounter == 1 && Projectile.frame > 4)
                {
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
            }
        }
        
        private void BoostedEffect()
        {
            //Spawn dust in circle
            int numberOfProjectiles = 8;
            for (float theta = 0; theta <= Math.Tau; theta += (float)Math.Tau / numberOfProjectiles)
            {
                Vector2 vel = Vector2.UnitX.RotatedBy(theta) * 2;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, vel, Scale: 2f);
                dust.noGravity = true;
            }
        }
    }
    public class IcecreamSandwich_WaferProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
        }
    }
}
