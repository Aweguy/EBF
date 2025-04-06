using EBF.Abstract_Classes;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Summon
{
    public class BloodBank : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 28;//Item's base damage value
            Item.knockBack = 4f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Green;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 2;

            Item.shoot = ModContent.ProjectileType<BloodBankStab>();
            BonusMinion = ModContent.ProjectileType<BloodBatMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 2;
        }

        //Dropped by Zombie Merman at 20%
    }

    public class BloodBankStab : ModProjectile
    {
        private const int projOffset = 8;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            DrawOffsetX = -4;
            DrawOriginOffsetY = -6;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Item item = Main.player[Projectile.owner].HeldItem;
            if (item.ModItem is EBFCatToy toy && !target.immortal)
            {
                toy.ApplyBoost(300);

                //Spawn fancy hit particle
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center });
            }
        }
        public override void PostAI()
        {
            Projectile.position += Projectile.velocity * projOffset;
        }
    }

    public class BloodBatMinion : EBFMinion
    {
        private const int dashSpeed = 5;
        private int healCooldownFrames = 60;
        public override string Texture => "EBF/Items/Summon/BloodBank_BloodBatMinion";
        public override bool MinionContactDamage() => true;
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 64;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            UseHoverAI = true;
            AttackRange = 40;
            AttackTime = 40;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath4, Projectile.Center);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (healCooldownFrames <= 0 && player.statLife < player.statLifeMax)
            {
                int healedHP = hit.Damage / 15;
                if (IsBoosted)
                {
                    healedHP *= 3;
                    BoostedEffect();
                }

                int type = ModContent.ProjectileType<BloodBatHealProjectile>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.UnitX.RotatedByRandom(MathHelper.Pi), type, 0, 0, Projectile.owner, Projectile.owner, healedHP);
                healCooldownFrames = 60;
            }
        }
        public override void AISafe()
        {
            Animate();
            healCooldownFrames--;

            //Max speed, which should probably be moved to base class.
            if (Projectile.velocity.Length() > MoveSpeed * 1.5f)
            {
                Projectile.velocity *= 0.9f;
            }

            Projectile.friendly = Target != null;
        }
        public override void OnAttack(NPC target)
        {
            Projectile.velocity += Projectile.DirectionTo(target.Center) * dashSpeed;
        }
        private void Animate()
        {
            if (Main.GameUpdateCount % 4 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 6)
                {
                    Projectile.frame = 0;
                }
            }
        }
        private void BoostedEffect()
        {
            SoundEngine.PlaySound(SoundID.AbigailAttack, Projectile.Center);

            //Spawn dust in circle
            int numberOfProjectiles = 8;
            for (float theta = 0; theta <= Math.Tau; theta += (float)Math.Tau / numberOfProjectiles)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(theta) * 2;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, velocity, Scale: 2f);
                dust.noGravity = true;
            }
        }
    }
    public class BloodBatHealProjectile : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CrimsonSpray}";
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Projectile.ExecuteHealingProjectileAI((int)Projectile.ai[1], (int)Projectile.ai[0], 6f, 15f);

            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, Alpha: 100, Scale: 2);
            dust.noGravity = true;
            dust.velocity = Projectile.velocity * 0.2f;
        }
    }
}
