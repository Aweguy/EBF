using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee.Throwable
{
    public class IceNeedle : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.WithFormatArgs("Ice Needle");//Name of the Item
        }

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;

            Item.damage = 40;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Melee;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
            Item.useTurn = false;

            Item.shoot = ModContent.ProjectileType<IceNeedle_Proj>();
            Item.shootSpeed = 16f;

            Item.noUseGraphic = true;
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
            int NumOfProjectiles = 9;
            float projRotation = 0f;
            #region Projectile spawn
            for (int p = 0; p <= NumOfProjectiles; p++)
            {
                Vector2 velocity = Projectile.oldVelocity.RotatedBy(projRotation * 0.0174533f) * 0.5f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, velocity, ModContent.ProjectileType<IceNeedle_Icicle>(), Projectile.damage / 2, Projectile.knockBack, Main.myPlayer);

                projRotation += 360 / NumOfProjectiles;
            }
            #endregion
            #region dust spawn
            Vector2 DustPosition = Projectile.position;
            Vector2 DustOldVelocity = Projectile.oldVelocity;
            DustOldVelocity.Normalize();
            DustPosition += DustOldVelocity * 16f;
            for (int i = 0; i < 30; i++)
            {
                int icy = Dust.NewDust(DustPosition, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 0, default(Color), 2f);
                Main.dust[icy].position = (Main.dust[icy].position + Projectile.Center) / 2f;
                Dust dust = Main.dust[icy];
                dust.velocity += Projectile.oldVelocity * 0.6f;
                dust = Main.dust[icy];
                dust.velocity *= 0.5f;
                Main.dust[icy].noGravity = true;
                DustPosition -= DustOldVelocity * 8f;
            }
            #endregion
        }
    }

    public class IceNeedle_Icicle : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.NorthPoleSnowflake}";

        public enum Behaviour//The behaviour of the snowflake
        {
            Idle = 0,
            Chase = 1
        }

        public Behaviour Behave
        {
            get => (Behaviour)behave;
            set => behave = (float)value;
        }
        private List<NPC> validNPCs;
        private float behave = 0f;
        private bool FrameFound;
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
            if (!FrameFound)//Setting the frame of the snowflake
            {
                FrameFound = true;
                Projectile.frame = Main.rand.Next(0, 2);

                //Get all valid npcs to target using the following criteria (reduces search size for homing)
                validNPCs = Main.npc.ToList<NPC>().FindAll(x => x.active && !x.dontTakeDamage && !x.friendly && x.lifeMax > 5);
            }
            if (Behave == Behaviour.Idle)//If the Projectile is idle then slow down smoothly
            {
                Projectile.velocity *= 0.90f;
            }
            else if (Behave == Behaviour.Chase)
            {
                Projectile.timeLeft = Projectile.timeLeft;
            }

            if (FindTarget(out Vector2 move) == true)
            {
                Behave = Behaviour.Chase;
                AdjustMagnitude(ref move);
                Projectile.velocity = (8 * Projectile.velocity + move) / 11f;
                AdjustMagnitude(ref Projectile.velocity);
            }
            else
            {
                Behave = Behaviour.Idle;
            }

            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            AdjustMagnitude(ref Projectile.velocity);
        }

        
        private bool FindTarget(out Vector2 move)
        {
            move = Vector2.Zero; //default case

            float distance = 125f;
            bool target = false;
            foreach (NPC npc in validNPCs)
            {
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

        private void AdjustMagnitude(ref Vector2 vector)
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
