using EBF.Abstract_Classes;
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
    public class SteelBuckler : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 12;//Item's base damage value
            Item.knockBack = 13f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 85, silver: 20, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 2;

            Item.shoot = ModContent.ProjectileType<SteelBucklerStab>();
            BonusMinion = ModContent.ProjectileType<CatSoldierMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 2;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.IronShortsword, stack: 1)
                .AddIngredient(ItemID.IronBar, stack: 12)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe(amount: 1)
               .AddIngredient(ItemID.LeadShortsword, stack: 1)
               .AddIngredient(ItemID.LeadBar, stack: 12)
               .AddTile(TileID.Anvils)
               .Register();
        }
    }

    public class SteelBucklerStab : ModProjectile
    {
        private const int projOffset = 10;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            DrawOffsetX = -2;
            DrawOriginOffsetY = -6;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Item item = Main.player[Projectile.owner].HeldItem;
            if(item.ModItem is EBFCatToy toy)
            {
                toy.ApplyBoost(180);
                
                //Spawn fancy hit particle
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center });
            }
        }
        public override void PostAI()
        {
            Projectile.position += Projectile.velocity * projOffset;
        }
    }

    public class CatSoldierMinion : EBFMinion
    {
        public override string Texture => "EBF/Items/Summon/SteelBuckler_CatSoldierMinion";
        public override void SetDefaultsSafe()
        {
            Projectile.width = 30;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            UseHoverAI = false;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item58, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Iron);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsBoosted)
            {
                modifiers.SetCrit();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsBoosted)
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
    }
}
