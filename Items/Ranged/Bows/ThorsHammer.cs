using EBF.Abstract_Classes;
using EBF.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class ThorsHammer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 22;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 66;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 41;//Item's base damage value
            Item.knockBack = 2.5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Ranged;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Shoot;//The animation of the item when used
            Item.useTime = 30;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 75, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item32;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
            Item.channel = true;
            Item.noMelee = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.HasAmmo(player.HeldItem) && !player.noItems && !player.CCed;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                type = ModContent.ProjectileType<ThorsHammer_Arrow>();
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 20)
                .AddIngredient(ItemID.SoulofSight, stack: 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class ThorsHammer_Arrow : EBFChargeableArrow
    {
        private int chainCount = 3; //How many times the projectile can choose a new target.
        private NPC target = null; //The target to chase, used to adjust arrow velocity and rotation.
        private List<NPC> hitTargets; //A list to keep track of all targets that's been previously hit, so they don't get tracked again.

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WoodenArrowFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.hide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.ignoreWater = true;

            MaximumDrawTime = 100;
            MinimumDrawTime = 20;

            DamageScale = 2f;
            VelocityScale = 2.5f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnProjectileRelease()
        {
            if (FullyCharged)
            {
                SoundEngine.PlaySound(SoundID.Item75, Projectile.position);
                hitTargets = new List<NPC>();
                Projectile.extraUpdates = 2;
                Projectile.penetrate = -1;
            }
        }
        public override void AI()
        {
            if (IsReleased && FullyCharged)
            {
                if (target != null)
                {
                    //Move towards target
                    Projectile.velocity = Vector2.Normalize(target.Center - Projectile.Center) * Projectile.velocity.Length();
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; //Accounting sprite facing up
                }

                //Trail
                Lighting.AddLight(Projectile.Center, TorchID.Yellow);
                for (int i = 0; i < 5; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, SpeedX: 0, SpeedY: 0, Scale: 2);
                    dust.noGravity = true;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (FullyCharged)
            {
                if (chainCount > 0)
                {
                    chainCount--;
                    hitTargets.Add(target);
                    if (!ProjectileExtensions.ClosestNPC(ref this.target, 500, Projectile.position, specialCondition: new ProjectileExtensions.SpecialCondition(CanTarget)))
                    {
                        Projectile.Kill();
                    }
                }
                else
                {
                    Projectile.Kill();
                }
            }
        }
        public bool CanTarget(NPC target) => !hitTargets.Contains(target);
    }
}
