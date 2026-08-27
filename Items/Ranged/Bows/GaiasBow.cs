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
    public class GaiasBow : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<GaiasBow_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 24;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 58;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 16;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 35;//How fast the item is used
            Item.useAnimation = 35;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Orange;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 7f;
            base.SetDefaults();
        }
        //Sold by Anna
    }

    public class GaiasBow_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<GaiasBow_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/GaiasBow";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 58;
            MaximumDrawTime = 60;
            DamageScale = 1.5f;
            VelocityScale = 2f;
            base.SetDefaults();
        }
    }

    public class GaiaSeed : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
                Projectile.ai[0] += 1f;

            int newWidth = (int)(18 * Projectile.scale);
            int newHeight = (int)(18 * Projectile.scale);
            Projectile.Resize(newWidth, newHeight);

            FadeInAndOut();
            { 
                Projectile.velocity.X = Projectile.velocity.X * 0.97f;
                Projectile.velocity.Y = Projectile.velocity.Y * 0.97f;
            }
        }
        public void FadeInAndOut()
        {
            // If last less than 50 ticks — fade in, than more — fade out
            if (Projectile.ai[0] <= 50f)
            {
                // Fade in
                Projectile.alpha -= 30;

                Projectile.scale += 0.01f;

                return;
            }

            // Fade out
            Projectile.alpha += 30;

            Projectile.scale -= 0.01f;

        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item60, Projectile.position);

            //Spawn dust in circle
            const int numberOfDusts = 8;
            for (float theta = 0; theta <= Math.Tau; theta += (float)Math.Tau / numberOfDusts)
            {
                var velocity = Vector2.UnitX.RotatedBy(theta) * 2;
                var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Plantera_Green, velocity, Scale: 2f);
                dust.noGravity = true;
            }

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * 5);

        }
        public override void OnKill(int timeLeft)
        {
            /*Temporary firework explosion until we care to make our own
            var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity / 2, ProjectileID.RocketFireworksBoxGreen, Projectile.damage = 0, Projectile.knockBack);
            proj.timeLeft = 0;*/
        }
    }
    public class GaiasBow_Arrow : ModProjectile
    {
        private const int gaiaSpawnRate = 6; //How often a projectile is spawned per second
        public override string Texture => "EBF/Items/Ranged/Bows/GaiasGift_Arrow";
        private bool fullyCharged;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = 3;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            fullyCharged = (int)Projectile.ai[0] == 1;
        }
        public override void AI()
        {
            //Prevent sub-projectiles from being spawned by other players' arrows.
            if (Main.myPlayer != Projectile.owner)
                return;

            //Run this code x times per second
            if (Main.GameUpdateCount % (60 / (gaiaSpawnRate)) == 0 && (fullyCharged)) 
                {
                    var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity / 2 + Main.rand.NextVector2Unit(), ModContent.ProjectileType<GaiaSeed>(), Projectile.damage, Projectile.knockBack);
                    proj.timeLeft = 100;
            }
            else if (Main.GameUpdateCount % (60 / gaiaSpawnRate*2) == 0 && (!fullyCharged))
                {
                    var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity / 2 + Main.rand.NextVector2Unit(), ModContent.ProjectileType<GaiaSeed>(), Projectile.damage, Projectile.knockBack);
                    proj.timeLeft = 100;
            }
        }
    }
}
