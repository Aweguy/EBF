using EBF.Abstract_Classes;
using EBF.Buffs;
using EBF.Extensions;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Summon
{
    public class StarHammer : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 38;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 46;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 214;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 10, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Red;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 12;

            Item.shoot = ModContent.ProjectileType<StarHammerStab>();
            BonusMinion = ModContent.ProjectileType<StarHammerMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 12;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Pwnhammer, stack: 1)
                .AddIngredient(ItemID.LunarBar, stack: 10)
                .AddIngredient(ItemID.FragmentStardust, stack: 10)
                .AddIngredient(ItemID.FallenStar, stack: 50)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class StarHammerStab : EBFToyStab
    {
        public override void SetDefaultsSafe()
        {
            DrawOffsetX = -12;
            DrawOriginOffsetY = -6;

            ProjOffset = 8;
            BoostDuration = 120;
            TagDamage = 6;
        }
    }

    public class StarHammerMinion : EBFMinion
    {
        private int cooldownFrames = 0;
        public override string Texture => "EBF/Items/Summon/StarHammer_ShootingStarMinion";
        public override bool MinionContactDamage() => true;
        public override void SetDefaultsSafe()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            UseHoverAI = true;
            AttackRange = 0;
            DetectRange = 800;
            MoveSpeed = 10f;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (cooldownFrames > 0)
                return;

            cooldownFrames = 60;

            ShootStarPattern();
        }
        public override void AISafe()
        {
            cooldownFrames--;

            if (IsBoosted && Main.GameUpdateCount % 40 == 0)
            {
                ShootStarPattern();
            }

            //Max speed, which should probably be moved to base class.
            if (Projectile.velocity.Length() > MoveSpeed * 1.5f)
            {
                Projectile.velocity *= 0.9f;
            }

            Projectile.friendly = Target != null;
        }

        private void ShootStarPattern()
        {
            int type = ProjectileID.SuperStar;
            float delta = (float)(Math.Tau / 5);
            float randomOffset = Main.rand.NextFloat(0, delta);

            for (float theta = randomOffset; theta < Math.Tau; theta += delta)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, ProjectileExtensions.PolarVector(6, theta), type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, ProjectileExtensions.PolarVector(10, theta + (delta / 2)), type, Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
    }
}
