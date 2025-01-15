using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Ranged.Bows
{
    public class GaiasBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged.Bows";
        public override void SetDefaults()
        {
            Item.width = 22;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 56;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 21;//Item's base damage value
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
            Item.shootSpeed = 7f;
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
                type = ModContent.ProjectileType<GaiasBow_Arrow>();
            }
        }
        //Sold by Witch Doctor
    }

    public class GaiasBow_Arrow : EBFChargeableArrow
    {
        public override string Texture => "EBF/Items/Ranged/Bows/GaiasGift_Arrow";
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

            MaximumDrawTime = 60;
            MinimumDrawTime = 15;

            DamageScale = 1.5f;
            VelocityScale = 2f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (FullyCharged)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GaiaSeed>(), 1, 0, Projectile.owner);
            }
        }
    }

    public class GaiaSeed : ModProjectile
    {
        private const int lifeTime = 2; //In seconds
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            //Spawn dust in circle
            int numberOfProjectiles = 8;
            for (float theta = 0; theta <= Math.Tau; theta += (float)Math.Tau / numberOfProjectiles)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(theta) * 2;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Plantera_Green, velocity, Scale: 2f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * 3);
        }
        public override void AI()
        {
            Projectile.alpha += (int)(255 / (60 * lifeTime)); //takes 1 * lifetime seconds
            if(Projectile.alpha >= 254)
            {
                Projectile.Kill();
            }
        }
    }
}
