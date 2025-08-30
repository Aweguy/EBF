using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class HeavensGate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 96;//Item's base damage value
            Item.knockBack = 5f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 34;//How fast the item is used
            Item.useAnimation = 34;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 9, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item80;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.shoot = ModContent.ProjectileType<HeavensGate_LightBlade>();
            Item.shootSpeed = 16f;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                var dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //ai0: how many times the projectile should copy itself.
            //ai1: how far the projectile should be offset from the spawn position
            Projectile.NewProjectile(source, Main.MouseWorld, velocity, type, damage, knockback, player.whoAmI, 2, 80);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Excalibur, stack: 1)
                .AddIngredient(ItemID.Ectoplasm, stack: 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class HeavensGate_LightBlade : ModProjectile
    {
        private bool animate = true; //Pauses animation while the projectile is moving forward
        private Vector2 spawnedPosition; //Used to check how far the projectile has travelled
        private ref float CopyLimit => ref Projectile.ai[0];
        private ref float CurrentCopyNumber => ref Projectile.localAI[0];
        private ref float SpawnPositionOffset => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1.3f;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.knockBack = 7f;
            Projectile.light = 1f;
            Projectile.tileCollide = false;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override bool? CanDamage() => Projectile.frame == 4;
        public override bool ShouldUpdatePosition() => Projectile.frame == 4;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.direction;

            //Offset the spawned position back and store position for distance checks
            Projectile.position -= Vector2.Normalize(Projectile.velocity) * SpawnPositionOffset;
            spawnedPosition = Projectile.position;
            Projectile.netUpdate = true;
        }
        public override void AI()
        {
            if (animate)
            {
                Animate();
            }
            else if (Vector2.Distance(spawnedPosition, Projectile.position) >= SpawnPositionOffset * 2)
            {
                animate = true;
            }

            //Create dust
            if (Main.rand.NextBool(3))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentCopyNumber < CopyLimit)
            {
                //Randomize direction
                float rotation = Main.rand.NextFloat(MathF.Tau);
                Vector2 velocity = Projectile.velocity.RotatedBy(rotation);

                //Spawn projectile
                var type = ModContent.ProjectileType<HeavensGate_LightBlade>();
                var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, velocity, type, hit.Damage, Projectile.knockBack, Projectile.owner, CopyLimit, SpawnPositionOffset);
                proj.localAI[0] = CurrentCopyNumber + 1;
            }
        }
        public override void OnKill(int timeLeft)
        {
            var velocity = Vector2.Normalize(Projectile.oldVelocity);
            var position = Projectile.position + velocity * 16f;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(position, Projectile.width, Projectile.height, DustID.AncientLight, velocity.X, velocity.Y);
                dust.noGravity = true;
            }
        }
        private void Animate()
        {
            //Advance frames every third tick
            if (Main.GameUpdateCount % 3 != 0)
                return;
            
            Projectile.frame++;
            switch (Projectile.frame)
            {
                case 4:
                    animate = false;
                    Projectile.netUpdate = true;
                    break;
                case 11:
                    Projectile.Kill();
                    break;
            }
        }
    }
}
