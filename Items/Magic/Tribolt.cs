using EBF.EbfUtils;
﻿using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace EBF.Items.Magic
{
    public class Tribolt : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 15;//Item's base damage value
            Item.knockBack = 3;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 6;//The amount of mana this item consumes on use

            Item.useTime = 5;//How fast the item is used
            Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 70, gold: 0, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item
            
            Item.shoot = ModContent.ProjectileType<Tribolt_Projectile>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Quirky way to shoot 2 times
            if (player.itemAnimation <= 25)
            {
                player.itemTime = 25;
            }

            //Spawn the projecile
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.AntlionMandible, stack: 3)
                .AddIngredient(ItemID.Amber, stack: 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class Tribolt_Projectile : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        private const int maxDistance = 400; //The distance the lightning can reach
        private const int attractionRange = 64; //The distance from the click, to search for and to snap onto found enemies
        private NPC target = null;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.timeLeft = 10;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 newPos;

            //Set projectile position within range limit
            if(Vector2.Distance(Projectile.Center, Main.MouseWorld) < maxDistance)
            {
                newPos = Main.MouseWorld;
            }
            else
            {
                newPos = Projectile.Center + Projectile.Center.DirectionTo(Main.MouseWorld) * maxDistance;
            }

            //Check for nearby enemies
            if (EBFUtils.ClosestNPC(ref target, attractionRange, newPos))
            {
                GenerateTrail(target);
                Projectile.Center = target.Center;
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
            }
            else
            {
                Projectile.Center = newPos;
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, Projectile.Center);
            }

            //Create dust
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric);
            }
        }
        private void GenerateTrail(NPC target)
        {
            Vector2
                a = Projectile.Center,
                b = Projectile.Center + Projectile.Center.DirectionTo(target.Center).RotatedByRandom(1.5) * 64,
                c = target.Center + target.Center.DirectionTo(Projectile.Center).RotatedByRandom(1.5) * 64,
                d = target.Center;
            
            Dust dust;

            if (Vector2.Distance(a, d) > 128)
            {
                //Trail a --> b
                for (int i = 0; i < Vector2.Distance(a, b); i += 8)
                {
                    dust = Dust.NewDustPerfect(a + a.DirectionTo(b) * i, DustID.Electric, Vector2.Zero);
                    dust.noGravity = true;
                }

                //Trail b --> c
                for (int i = 0; i < Vector2.Distance(b, c); i += 8)
                {
                    dust = Dust.NewDustPerfect(b + b.DirectionTo(c) * i, DustID.Electric, Vector2.Zero);
                    dust.noGravity = true;
                }

                //Trail c --> d
                for (int i = 0; i < Vector2.Distance(c, d); i += 8)
                {
                    dust = Dust.NewDustPerfect(c + c.DirectionTo(d) * i, DustID.Electric, Vector2.Zero);
                    dust.noGravity = true;
                }
            }
            else
            {
                //Trail a --> d
                for (int i = 0; i < Vector2.Distance(a, d); i += 8)
                {
                    dust = Dust.NewDustPerfect(a + a.DirectionTo(d) * i, DustID.Electric, Vector2.Zero);
                    dust.noGravity = true;
                }
            }
        }
    }
}
