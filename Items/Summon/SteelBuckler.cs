﻿using EBF.Abstract_Classes;
using EBF.Buffs;
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

            Item.damage = 14;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
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

    public class SteelBucklerStab : EBFToyStab
    {
        public override void SetDefaultsSafe()
        {
            DrawOffsetX = -2;
            DrawOriginOffsetY = -6;

            ProjOffset = 10;
            BoostDuration = 180;
            TagDamage = 2;
        }
    }

    public class CatSoldierMinion : EBFMinion
    {
        public override string Texture => "EBF/Items/Summon/SteelBuckler_CatSoldierMinion";
        public override bool MinionContactDamage() => true;
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 11;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 50;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            UseHoverAI = false;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item58.WithVolumeScale(0.3f), Projectile.Center);
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
            //Use attack animation
            Projectile.ai[0] = 2;
            Projectile.frame = 8;

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
        public override void AISafe()
        {
            //Transitions between idle and walk
            if (Math.Abs(Projectile.velocity.X) > 1.5f && Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                Projectile.frame = 4;
            }
            else if (Math.Abs(Projectile.velocity.X) < 1f && Projectile.ai[0] == 1)
            {
                Projectile.ai[0] = 0;
                Projectile.frame = 0;
            }

            Projectile.friendly = Target != null;
            Animate();
        }
        private void Animate()
        {
            if (Main.GameUpdateCount % 6 == 0)
            {
                Projectile.frame++;
                switch (Projectile.ai[0])
                {
                    case 0: //Idle animation
                        if (Projectile.frame >= 4)
                        {
                            Projectile.frame = 0;
                        }
                        break;

                    case 1: //Walk animation
                        if (Projectile.frame >= 8)
                        {
                            Projectile.frame = 4;
                        }
                        break;

                    case 2: //Attack animation
                        if (Projectile.frame >= 11)
                        {
                            Projectile.frame = 0;
                            Projectile.ai[0] = 0;
                        }
                        break;

                }
            }
        }
    }
}
