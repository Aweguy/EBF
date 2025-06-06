using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Machines
{
    public class HarpoonTurret : Turret
    {
        private Projectile hook;
        private ref float Timer => ref NPC.localAI[0];
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Type] = 2;
        }
        public override void SetDefaultsSafe()
        {
            NPC.width = 108;
            NPC.height = 54;
            NPC.damage = 30;
            NPC.defense = 18;
            NPC.lifeMax = 2000;
        }
        public override void AISafe()
        {
            Player player = Main.player[NPC.target];
            Timer++;

            LerpRotationToTarget(player, 0.1f);

            if(Timer == 240)
            {
                IsShooting = 1;
                Shoot();
            }

            //Reset when hook is destroyed (fully retracted)
            if(IsShooting == 1 && (hook == null || !hook.active))
            {
                IsShooting = 0;
                Timer = 0;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame = new(0, 0, NPC.width, NPC.height)
            {
                Y = IsShooting == 1 ? 54 : 0 // frameHeight is fucked for some reason, maybe it's cuz I draw manually?
            };
        }

        public override void OnKillSafe()
        {
            for (int i = 0; i < 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 4).RotatedByRandom(1f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore{i}").Type, NPC.scale);
        
            if(IsShooting == 0)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 4).RotatedByRandom(1f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore2").Type, NPC.scale);
            }
        }

        private void Shoot()
        {
            var speed = 20;
            var vel = NPC.rotation.ToRotationVector2() * speed;
            var type = ModContent.ProjectileType<HarpoonTurret_Projectile>();
            hook = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, vel, type, NPC.damage / 2, 3f, -1, NPC.whoAmI);

            SoundEngine.PlaySound(SoundID.Item11, NPC.position);
        }
    }

    public class HarpoonTurret_Projectile : ModProjectile
    {
        private const int RetractionSpeed = 12;
        private const int AirTimeLimit = 120;
        private bool isRetracting;
        private NPC Parent => Main.npc[(int)Projectile.ai[0]];
        private Vector2 ParentFront => Parent.Center + Parent.rotation.ToRotationVector2() * 32;
        private ref float Timer => ref Projectile.localAI[0];
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            isRetracting = true;
            Projectile.tileCollide = false;

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return false;
        }
        public override void AI()
        {
            if(Parent == null || !Parent.active)
            {
                Projectile.Kill();
            }

            if (isRetracting)
            {
                Projectile.velocity = Projectile.DirectionTo(ParentFront) * RetractionSpeed;
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

                if(Vector2.Distance(Projectile.Center, ParentFront) < 16)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.velocity.Y += 0.1f;
                Projectile.velocity.X *= 0.99f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Timer++;
                if (Timer > AirTimeLimit)
                {
                    isRetracting = true;
                    Projectile.tileCollide = false;
                }
            }
        }
        public override bool PreDrawExtras()
        {
            var chainTexture = Terraria.GameContent.TextureAssets.Chain.Value;
            var start = ParentFront;
            var end = Projectile.Center;

            var direction = end - start;
            var length = direction.Length();
            direction.Normalize();

            var chainSegmentLength = chainTexture.Height;

            var position = start;
            var rotation = direction.ToRotation() - MathHelper.PiOver2;

            for (float i = 0; i <= length; i += chainSegmentLength)
            {
                Main.spriteBatch.Draw(
                    chainTexture,
                    position - Main.screenPosition,
                    null,
                    Lighting.GetColor((int)(position.X / 16), (int)(position.Y / 16)),
                    rotation,
                    new Vector2(chainTexture.Width / 2f, chainTexture.Height / 2f),
                    1f,
                    SpriteEffects.None,
                    0f
                );

                position += direction * chainSegmentLength;
            }

            return false;
        }
    }
}
