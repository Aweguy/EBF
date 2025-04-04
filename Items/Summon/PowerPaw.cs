﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using EBF.Extensions;

namespace EBF.Items.Summon
{
    public class PowerPaw : EBFCatToy, ILocalizedModType
    {
        private int projAnimAI = 0; //Allows us to alternate the animation the punch projectile uses.
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 36;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 63;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 16;//How fast the item is used
            Item.useAnimation = 16;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 50, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 4;

            Item.shoot = ModContent.ProjectileType<PowerPawPunch>();
            BonusMinion = ModContent.ProjectileType<OrigamiDragonMinion>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, projAnimAI);
            projAnimAI = projAnimAI == 0 ? 1 : 0;
            return false;
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 6;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.BambooBlock, stack: 200)
                .AddIngredient(ItemID.TatteredCloth, stack: 3)
                .AddIngredient(ItemID.SoulofMight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class PowerPawPunch : ModProjectile
    {
        private const int projOffset = 4;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 12;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Item item = Main.player[Projectile.owner].HeldItem;
            if (item.ModItem is EBFCatToy toy)
            {
                toy.ApplyBoost(60);

                //Spawn fancy hit particle
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center });
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Projectile.ai[0] == 0 ? 0 : 6;
            Projectile.LookAt(Main.MouseWorld);
        }
        public override void AI()
        {
            Projectile.Center = Main.LocalPlayer.Center + Projectile.velocity * projOffset;
            Animate();
        }
        private void Animate()
        {
            if (Main.GameUpdateCount % 3 == 0)
            {
                Projectile.frame++;
                if(Projectile.frame >= 6 && Projectile.ai[0] == 0)
                {
                    Projectile.Kill();
                }
                else if(Projectile.frame >= 12)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
