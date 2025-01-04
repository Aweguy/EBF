using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee.Throwable
{
    public class IceNeedle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;

            Item.damage = 72;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Melee;

            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 15, platinum: 0);//Item's value when sold
            Item.useTurn = false;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IceNeedle_Proj>();
            Item.shootSpeed = 16f;

            Item.noUseGraphic = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.NorthPole, stack: 1)
                .AddIngredient(ItemID.FragmentSolar, stack: 12)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class IceNeedle_Proj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 26;

            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 30;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            AIType = ProjectileID.JavelinFriendly;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            //Fix incorrect sprite rotation
            float velRotation = Projectile.velocity.ToRotation();
            Projectile.rotation = velRotation + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

            int numberOfProjectiles = 9;
            float projRotation = 0f;

            #region Projectile spawn
            for (int p = 0; p <= numberOfProjectiles; p++)
            {
                Vector2 velocity = Projectile.oldVelocity.RotatedBy(projRotation) * 0.5f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, velocity, ModContent.ProjectileType<IceNeedle_Icicle>(), Projectile.damage / 2, Projectile.knockBack, Main.myPlayer);

                projRotation += (float)Math.Tau / numberOfProjectiles;
            }
            #endregion

            #region Dust spawn
            Vector2 dustPosition = Projectile.position;
            Vector2 dustOldVelocity = Vector2.Normalize(Projectile.oldVelocity);
            dustPosition += dustOldVelocity * 16f;

            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(dustPosition, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 0, default, 2f);
                dust.position = (dust.position + Projectile.Center) / 2f;
                dust.velocity += Projectile.oldVelocity * 0.3f;
                dust.noGravity = true;

                //Form dust into a javelin shape
                dustPosition -= dustOldVelocity * 8f;
            }
            #endregion
        }
    }

    public class IceNeedle_Icicle : ModProjectile
    {
        private List<NPC> validNPCs; //This is used to reduce how many npcs are checked in the homing logic
        private bool isChasing = false;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.NorthPoleSnowflake}";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 26;

            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.timeLeft = 60 * 2;

            Projectile.light = 1f;
            Projectile.tileCollide = false;
        }
        public override bool PreAI()
        {
            if (isChasing)
            {
                //Don't drain projectile lifetime
                Projectile.timeLeft = Projectile.timeLeft;
            }
            else
            {
                //Slow down over time
                Projectile.velocity *= 0.9f;
            }

            if (FindTarget(out Vector2 move) == true)
            {
                isChasing = true;
                AdjustMagnitude(ref move);
                Projectile.velocity = (8 * Projectile.velocity + move) / 11f;
                AdjustMagnitude(ref Projectile.velocity);
            }
            else
            {
                isChasing = false;
            }

            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, 3);
            AdjustMagnitude(ref Projectile.velocity);

            //Get all valid npcs to target using the following criteria (reduces search size for homing)
            validNPCs = Main.npc.ToList<NPC>().FindAll(x => x.active && !x.dontTakeDamage && !x.friendly && x.lifeMax > 5);
        }
        private bool FindTarget(out Vector2 move)
        {
            move = Vector2.Zero; //default case

            float distance = 125f;
            bool target = false;
            foreach (NPC npc in validNPCs)
            {
                //Ignore dead npcs
                if (npc.life <= 0)
                {
                    continue;
                }

                Vector2 towardsNPC = npc.Center - Projectile.Center;
                float distanceTo = towardsNPC.Length();
                if (distanceTo < distance)
                {
                    move = towardsNPC;
                    distance = distanceTo;
                    target = true;
                }
            }
            return target;
        }
        private static void AdjustMagnitude(ref Vector2 vector)
        {
            float magnitude = vector.Length();
            if (magnitude > 6f)
            {
                vector *= 9f / magnitude;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

            for (int i = 0; i <= 6; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Projectile.oldVelocity.X, Projectile.oldVelocity.Y);
            }
        }
    }
}
