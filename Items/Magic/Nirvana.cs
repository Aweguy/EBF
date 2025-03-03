using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System.Collections.Generic;
using System;

namespace EBF.Items.Magic
{
    public class Nirvana : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaultsSafe()
        {
            Item.width = 54;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 54;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 60;//Item's base damage value
            Item.knockBack = 4;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.mana = 8;//The amount of mana this item consumes on use

            Item.useTime = 48;//How fast the item is used
            Item.useAnimation = 48;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 2, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Lime;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = false;//Boolean, if the player's direction can change while using the item

            Item.UseSound = SoundID.Item66;
            Item.shoot = ModContent.ProjectileType<Nirvana_Projectile>();
            Item.shootSpeed = 16f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = StaffHead.DirectionTo(Main.MouseWorld) * Item.shootSpeed;

            //Spawn the projecile
            Projectile.NewProjectile(source, StaffHead, velocity, type, damage, knockback, player.whoAmI, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient<DruidStaff>(stack: 1)
                .AddIngredient(ItemID.SpookyWood, stack: 40)
                .AddIngredient(ItemID.Stinger, stack: 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class Nirvana_Projectile : ModProjectile
    {
        float vineRotationOffset = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 70;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
        }
        public override void AI()
        {
            //Slow over time
            Projectile.velocity *= 0.95f;

            //Animation
            if (Main.GameUpdateCount % 8 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            //Vine spawns
            if(Projectile.timeLeft < 40 && Main.GameUpdateCount % 10 == 0 && vineRotationOffset < 1.5f)
            {
                SoundEngine.PlaySound(SoundID.Item17);

                for (int i = 0; i < 3; i++)
                {
                    int type = ModContent.ProjectileType<Nirvana_MonsterVine>();
                    Vector2 velocity = Vector2.UnitX.RotatedBy(Math.Tau / 3 * i + vineRotationOffset); //Triangle pattern + an offset that increases after the loop

                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    proj.Center = Projectile.Center;
                    proj.position += proj.velocity * 128;
                    proj.rotation = proj.velocity.ToRotation() + MathHelper.PiOver2;
                    proj.velocity = Projectile.velocity;

                }

                vineRotationOffset += 0.5f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.JungleGrass, Scale: 2);
                dust.noGravity = true;
            }
        }
        public class Nirvana_MonsterVine : ModProjectile
        {
            private int[] frameSequence = { 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0 };
            public override void SetStaticDefaults()
            {
                Main.projFrames[Projectile.type] = 6;
                ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
            }
            public override void SetDefaults()
            {
                Projectile.width = 32;
                Projectile.height = 96;
                Projectile.friendly = true;
                Projectile.tileCollide = false;
                Projectile.DamageType = DamageClass.Generic;
                Projectile.penetrate = -1;
                Projectile.hide = true;
            }
            public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
            {
                behindProjectiles.Add(index);
            }
            public override void AI()
            {
                //Handle animation
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter > 10)
                    {
                        Projectile.Kill();
                    }
                    else
                    {
                        Projectile.frame = frameSequence[Projectile.frameCounter];
                    }
                }
            }
        }
    }
}
