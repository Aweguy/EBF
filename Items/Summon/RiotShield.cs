using EBF.Abstract_Classes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Drawing;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using EBF.Extensions;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using EBF.Buffs;
using EBF.NPCs.Machines;

namespace EBF.Items.Summon
{
    public class RiotShield : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 42;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 48;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 64;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 3, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Pink;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 10;

            Item.shoot = ModContent.ProjectileType<RiotShieldStab>();
            BonusMinion = ModContent.ProjectileType<RedFlybotMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 10;
            player.velocity.X = MathHelper.Clamp(player.velocity.X, -5.5f, 5.5f);
            player.velocity.Y = MathHelper.Clamp(player.velocity.Y, -8f, 12f);
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.HallowedBar, stack: 10)
                .AddIngredient(ItemID.SoulofMight, stack: 5)
                .AddIngredient(ItemID.SoulofSight, stack: 5)
                .AddIngredient(ItemID.SoulofFright, stack: 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class RiotShieldStab : EBFToyStab
    {
        public override void SetDefaultsSafe()
        {
            DrawOffsetX = 0;
            DrawOriginOffsetY = -6;

            ProjOffset = 4;
            BoostDuration = 120;
            TagDamage = 4;
        }
    }

    public class RedFlybotMinion : EBFMinion
    {
        private int gunToUse;
        private RedFlybotCannon[] cannons = new RedFlybotCannon[2];
        public override string Texture => "EBF/NPCs/Machines/RedFlybot";
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            DetectRange = 600;
            AttackRange = 400;
            AttackTime = 20;
            MoveSpeed = 5f;
            UseHoverAI = true;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item113, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
            }

            //Spawn and store the cannons
            int type = ModContent.ProjectileType<RedFlybotCannon>();

            for (int i = 0; i < cannons.Length; i++)
            {
                int proj = Projectile.NewProjectile(source, Projectile.Center, Vector2.UnitX, type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: i);
                cannons[i] = Main.projectile[proj].ModProjectile as RedFlybotCannon;
                cannons[i].Parent = this;
            }
        }
        public override void AISafe()
        {
            //Animation
            if (Main.GameUpdateCount % 4 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame > 2)
                {
                    Projectile.frame = 0;
                }
            }
        }
        public override void OnAttack(NPC target)
        {
            gunToUse = gunToUse == 0 ? 1 : 0; //Swap gun
            cannons[gunToUse].Shoot(target);

            if (IsBoosted)
            {
                AttackTime = 5;
            }
            else
            {
                AttackTime = 20;
            }
        }
    }

    public class RedFlybotCannon : ModProjectile
    {
        private Vector2 recoilOffset;
        public RedFlybotMinion Parent { get; set; }

        public override string Texture => "EBF/NPCs/Machines/RedFlybot_Cannon";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 24;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.hide = Projectile.ai[0] == 1;
        }
        public override void AI()
        {
            if (!Parent.Projectile.active || Parent.Type != ModContent.ProjectileType<RedFlybotMinion>())
            {
                Projectile.Kill();
            }

            if(recoilOffset.Length() > 0)
            {
                recoilOffset *= 0.9f;
            }

            //Stick to flybot
            float offset = Projectile.ai[0] == 1 ? 16f : -16f;
            Projectile.Center = Parent.Projectile.Center + new Vector2((offset * Projectile.spriteDirection) + 16, 10);
            Projectile.position += recoilOffset;

            //Rotation
            Projectile.direction = Projectile.spriteDirection = Math.Sign(Parent.Projectile.velocity.X);

            if (Parent.Target != null)
            {
                Projectile.LookAt(Parent.Target.position);
            }
            else
            {
                Projectile.rotation = Parent.Projectile.rotation;
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.ai[0] == 1)
            {
                behindProjectiles.Add(index);
            }
        }

        public void Shoot(NPC target)
        {
            Vector2 velocity = Projectile.Center.DirectionTo(target.Center) * 14;
            int type = ModContent.ProjectileType<RedFlybot_Laser>();

            SoundEngine.PlaySound(SoundID.Item158, Projectile.Center);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);

            //Extra juice
            recoilOffset -= Projectile.velocity * 16;
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch, velocity.X, velocity.Y, Scale: 2.5f);
                dust.noGravity = true;
            }
        }
    }
}
