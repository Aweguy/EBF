using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class Inferno : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 26;//Item's base damage value
            Item.knockBack = 1f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Rapier;//The animation of the item when used
            Item.useTime = 6;//How fast the item is used
            Item.useAnimation = 18;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Orange;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item

            Item.noUseGraphic = true; // Important, because otherwise you'd sometimes see a duplicate item sprite
            Item.shoot = ModContent.ProjectileType<Inferno_Proj>();
            Item.shootSpeed = 2;
            Item.noMelee = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Gladius, stack: 1)
                .AddIngredient(ItemID.HellstoneBar, stack: 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Shortswords act as projectiles. So this class represents the blade that is seen upon using the Inferno item.
    /// </summary>
    public class Inferno_Proj : ModProjectile
    {
        private const float positionOffset = 40f;
        private float rotationOffset;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30; //sprite is 83, but hitbox wants to stay facing up.
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.penetrate = -1;

            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            rotationOffset = Main.rand.NextFloat(-0.33f, 0.33f);
            Projectile.velocity = Projectile.velocity.RotatedBy(rotationOffset);
            Projectile.rotation += rotationOffset;
        }
        public override void PostAI() //Post because otherwise the shortsword aiStyle would override any changes
        {
            Projectile.position += Vector2.Normalize(Projectile.velocity) * positionOffset;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, 360)) * 4;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, velocity, ModContent.ProjectileType<Inferno_Fireball>(), Projectile.damage / 2, KnockBack: 0);
            }

            target.AddBuff(BuffID.OnFire, 60 * 2);
        }
    }

    public class Inferno_Fireball : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;

            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 60 * 2;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.OnFire, 60 * 2);
            }
        }

        public override bool PreAI()
        {
            Projectile.velocity *= 0.95f;

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, Color.Orange, 1f);
                dust.noGravity = true;
            }

            return false;
        }
    }
}
