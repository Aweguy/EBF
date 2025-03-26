﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Summon
{
    public class GodlyBook : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 28;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 30;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 68;//Item's base damage value
            Item.knockBack = 13f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 20, gold: 1, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.LightRed;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 5;

            Item.shoot = ModContent.ProjectileType<GodlyBookStab>();
            BonusMinion = ModContent.ProjectileType<AngelMirrorMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 5;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.SpellTome, stack: 1)
                .AddIngredient(ItemID.SoulofNight, stack: 15)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }

    public class GodlyBookStab : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.ShortSword;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            DrawOffsetX = -6;
            DrawOriginOffsetY = -6;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Item item = Main.player[Projectile.owner].HeldItem;
            if (item.ModItem is EBFCatToy toy)
            {
                toy.ApplyBoost(120);

                //Spawn fancy hit particle
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center });
            }
        }
    }

    public class AngelMirrorMinion : EBFMinion
    {
        public override string Texture => "EBF/Items/Summon/GodlyBook_AngelMirrorMinion";
        public override void SetDefaultsSafe()
        {
            Projectile.width = 84;
            Projectile.height = 66;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            UseHoverAI = true;
            AttackRange = 300;
            AttackTime = 40;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item53, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Gold);
            }
        }
        public override void OnAttack(NPC target)
        {
            Vector2 velocity = Projectile.DirectionTo(target.Center) * 8f;
            if (IsBoosted)
            {
                velocity = velocity.RotateRandom(0.4f);
                AttackTime = 8;
            }
            else
            {
                AttackTime = 40;
            }

            var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileID.DiamondBolt, Projectile.damage, 0);
            proj.usesLocalNPCImmunity = true;
            proj.localNPCHitCooldown = -1;
        }
    }
}
