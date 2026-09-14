using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class HeavensVoice : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<HeavensVoice_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 46;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 50;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 41;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 75, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<ThorsHammer>(stack: 1)
                .AddIngredient(ItemID.Harp, stack: 1)
                .AddIngredient(ItemID.PixieDust, stack: 20)
                .AddIngredient(ItemID.HallowedBar, stack: 20)
                .AddIngredient(ItemID.BeetleHusk, stack: 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class HeavensVoice_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<HeavensVoice_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/HeavensVoice";
        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 50;
            MaximumDrawTime = 70;
            DamageScale = 2f;
            VelocityScale = 2f;
            base.SetDefaults();
        }
    }

    public class HeavensVoice_Arrow : ModProjectile
    {
        private bool fullyCharged;
        private int chainCount = 9; //How many times the projectile can choose a new target.
        private NPC target = null; //The target to chase, used to adjust arrow velocity and rotation.
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            Projectile.localNPCHitCooldown = 250;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            fullyCharged = (int)Projectile.ai[0] == 1;
            SoundEngine.PlaySound(SoundID.Item26, Projectile.position);
            if (!fullyCharged)
                return;

            Projectile.tileCollide = false;

            Projectile.extraUpdates = 10;
            Projectile.penetrate = -1;


        }
        public override bool PreAI()
        {
            CreateTrail();
            SetTarget();

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (target == null)
            {
                Projectile.velocity += Vector2.UnitY * 0.08f;
            }

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fullyCharged)
                return;

            // Change target
            if (chainCount > 0)
            {
                chainCount--;
                if (!EBFUtils.ClosestNPC(ref this.target, 2000, Projectile.position))
                        Projectile.velocity = Projectile.velocity * 2;    
            }
            else
                Projectile.Kill();
        }

        private void SetTarget()
        {


            //Limit how often we search for targets for performance
            //And delay beginning check for extra flair
            if (Projectile.ai[0] > 50)
            {
                    if (EBFUtils.ClosestNPC(ref target, 2000, Projectile.position))
                    {
                        Projectile.HomeTowards(target, maxSpeed: 10, strength: 1);
                    }
                    else
                    {
                        target = null;
                    }
            }
        }
        private void CreateTrail()
        {
            Lighting.AddLight(Projectile.Center, TorchID.Blue);
            for (var i = 0; i < 5; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, SpeedX: 0, SpeedY: 0, Scale: 2);
                dust.noGravity = true;
            }
        }
    }
}
