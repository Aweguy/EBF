using EBF.Abstract_Classes;
using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class ThorsHammer : EBFBow, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        protected override int HoldoutProjectile => ModContent.ProjectileType<ThorsHammer_HoldoutProjectile>();

        public override void SetDefaults()
        {
            Item.width = 22;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 41;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 75, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.shootSpeed = 8f;
            base.SetDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddRecipeGroup("TitaniumBar", stack: 20)
                .AddIngredient(ItemID.SoulofLight, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class ThorsHammer_HoldoutProjectile : EBFHoldoutBow
    {
        protected override int ArrowType => ModContent.ProjectileType<ThorsHammer_Arrow>();
        public override string Texture => "EBF/Items/Ranged/Bows/ThorsHammer";
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 66;
            MaximumDrawTime = 70;
            DamageScale = 2f;
            VelocityScale = 2.5f;
            base.SetDefaults();
        }
    }

    public class ThorsHammer_Arrow : ModProjectile
    {
        private bool fullyCharged;
        private int chainCount = 3; //How many times the projectile can choose a new target.
        private NPC target = null; //The target to chase, used to adjust arrow velocity and rotation.
        private List<NPC> hitTargets = []; //A list to keep track of all targets that's been previously hit, so they don't get tracked again.
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

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
            if (!fullyCharged)
                return;
            
            Projectile.extraUpdates = 2;
            Projectile.penetrate = -1;
            SoundEngine.PlaySound(SoundID.Item75, Projectile.position);
        }

        public override void AI()
        {
            if (!fullyCharged)
                return;
            
            if (target != null)
            {
                //Move towards target
                Projectile.velocity = Vector2.Normalize(target.Center - Projectile.Center) * Projectile.velocity.Length();
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; //Accounting sprite facing up
            }

            CreateTrail();
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fullyCharged)
                return;
            
            // Change target
            if (chainCount > 0)
            {
                chainCount--;
                hitTargets.Add(target);
                if (!EBFUtils.ClosestNPC(ref this.target, 500, Projectile.position, specialCondition: new EBFUtils.SpecialCondition(CanTarget)))
                    Projectile.Kill();
            }
            else
                Projectile.Kill();
        }
        
        private bool CanTarget(NPC target) => !hitTargets.Contains(target);
        private void CreateTrail()
        {
            Lighting.AddLight(Projectile.Center, TorchID.Yellow);
            for (var i = 0; i < 5; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, SpeedX: 0, SpeedY: 0, Scale: 2);
                dust.noGravity = true;
            }
        }
    }
}
