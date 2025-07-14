using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using EBF.EbfUtils;
using EBF.Items.Materials;
using EBF.Items.Ranged.Guns;
using EBF.Abstract_Classes;

namespace EBF.Items.Magic
{
    public class WreckingRod : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 40;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 61;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 12;//The amount of mana this item consumes on use

            Item.useTime = 8;//How fast the item is used
            Item.useAnimation = 38;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<WreckingRod_MagicGrenade>();
            Item.shootSpeed = 12f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Quirky way to shoot 3 times
            if (player.itemAnimation <= 30)
            {
                player.itemTime = 30;
            }

            //Throw
            velocity = StaffHead.DirectionTo(Main.MouseWorld).RotatedByRandom(0.1f) * Item.shootSpeed;
            velocity.Y -= 4f;

            //Spawn the projecile
            SoundEngine.PlaySound(SoundID.Item9, player.Center);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ModContent.ItemType<NanoFibre>(), stack: 2)
                .AddIngredient(ModContent.ItemType<RamChip>(), stack: 22)
                .AddIngredient(ItemID.ExplosivePowder, stack: 180)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class WreckingRod_MagicGrenade : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.Explosive[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.aiStyle = ProjAIStyleID.Explosive;
            AIType = ProjectileID.GrenadeI; //This manages OnNPCHit for some reason

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.timeLeft = 40;
            Projectile.penetrate = 1;
        }
        public override void OnKill(int timeLeft)
        {
            //Explode
            Projectile.CreateExplosionEffect(EBFUtils.ExplosionSize.Small);
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            //Create plasma burst
            var type = ModContent.ProjectileType<PositronRifle_PlasmaBurst>();
            var proj = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
            proj.Center = Projectile.Center;
            proj.DamageType = DamageClass.Magic;
        }
    }
}
