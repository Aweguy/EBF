using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    [AutoloadBossHead]
    public class Godcat_Destroyer : Godcat_Vehicle
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 8;

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/Godcat_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 152;
            NPC.height = 152;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.DeathSound = SoundID.NPCDeath5;
            idleTexture = ModContent.Request<Texture2D>(Texture);
            attackTexture = ModContent.Request<Texture2D>(Texture + "_Attack");
            currentTexture = idleTexture.Value;

            stateDurations = new()
            {
                [State.Idle] = 200,
                [State.TurningBallCircle] = 240,
                [State.DestroyerBreath] = 200,
                [State.DestroyerBallBurst] = 10,
                [State.DestroyerHomingBall] = 130,
                [State.DestroyerFireWheel] = 1,
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Destroyer")
            ]);
        }
        public override void AI()
        {
            base.AI();
            var player = Main.player[NPC.target];
            switch (currentState)
            {
                case State.TurningBallCircle:
                    CreateTurningBallsCircles();
                    break;
                case State.DestroyerBreath:
                    CreateDarkBreath(player);
                    break;
                case State.DestroyerBallBurst:
                    CreateMassiveBallBurst(player);
                    break;
                case State.DestroyerHomingBall:
                    CreateDarkHomingBall(player);
                    break;
                case State.DestroyerFireWheel:
                    CreateFireWheel(player);
                    break;
            }
        }
        protected override void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(-550, -100);
            NPC.Center = Vector2.Lerp(NPC.Center, preferredPosition, 0.05f);
        }
        protected override void BeginNextPhase(Player player)
        {
            //Spawn light godcat
            var type = ModContent.NPCType<Godcat_Light>();
            var pos = player.Center.ToPoint() + new Point(NPC.direction * 1600, 0);
            NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type, 0, Phase + 1);

            //Spawn dark godcat
            var type2 = ModContent.NPCType<Godcat_Dark>();
            var pos2 = player.Center.ToPoint() + new Point(-NPC.direction * 1600, 0);
            NPC.NewNPC(NPC.GetSource_FromAI(), pos2.X, pos2.Y, type2, 0, Phase + 1);
        }
        private void CreateTurningBallsCircles()
        {
            if (Main.GameUpdateCount % 30 == 0)
            {
                var amount = 12;
                var speed = 5;
                var type = ModContent.ProjectileType<Godcat_TurningBall>();
                for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
                {
                    var velocity = Vector2.UnitX.RotatedBy(theta) * speed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.DarkBig, -0.005f);

                    if (IsAlone)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity * 0.9f, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.DarkBig, 0.005f);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
            }
        }
        private void CreateMassiveBallBurst(Player player)
        {
            //Begin attack animation
            if (StateTimer == 0)
            {
                SetAnimation(attackTexture, 12);
            }
            //Shoot projectiles
            else if (StateTimer == 9)
            {
                var spread = 0.2f;
                var speed = 8f;
                var speedRange = 0.2f;
                var baseVelocity = NPC.DirectionTo(player.Center) * speed;
                var type = ModContent.ProjectileType<Godcat_BallProjectile>();
                for (int i = 0; i < 40; i++)
                {
                    var velocity = baseVelocity.RotatedByRandom(spread) * Main.rand.NextFloat(1 - speedRange, 1 + speedRange);
                    var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.DarkBig);
                    proj.scale = Main.rand.NextFloat(0.5f, 1.5f);
                }

                SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound

                //Additional arc of projectiles
                CreateBallArc(player, 1.5f, 9, 5f);
                CreateBallArc(player, 1.5f, 8, 4f);
            }
        }
        private void CreateBallArc(Player player, float spread, int amount, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_BallProjectile>();
            for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
            {
                var velocity = NPC.DirectionTo(player.Center).RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.DarkSmall);
            }
        }
        private void CreateDarkHomingBall(Player player)
        {
            //Begin attack animation
            if (StateTimer % 59 == 0)
            {
                SetAnimation(attackTexture, 12);
            }
            //Shoot projectile
            if (StateTimer % 59 == 10)
            {
                var speed = 4f;
                var velocity = NPC.DirectionTo(player.Center) * speed;
                var type = ModContent.ProjectileType<Destroyer_DarkHomingBall>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, player.whoAmI);

                if (IsAlone)
                {
                    CreateBallArc(player, 1f, 4, 8f);
                }

                SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
            }
        }
        private void CreateDarkBreath(Player player)
        {
            var windupTime = 50;
            if (StateTimer == 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath60, NPC.position);
                SetAnimation(attackTexture, 2);
            }
            else if (StateTimer == windupTime)
            {
                SoundStyle sound = SoundID.NPCHit57;
                sound.Pitch = -1.0f;
                sound.Volume = 0.5f;
                SoundEngine.PlaySound(sound, NPC.position);
            }

            //Shoot projectiles
            else if (StateTimer > windupTime && Main.GameUpdateCount % 10 == 0)
            {
                var speed = 7f * Main.rand.NextFloat(0.8f, 1.2f);
                var velocity = NPC.DirectionTo(player.Center).RotatedByRandom(0.5f) * speed + player.velocity * 0.75f;
                var type = ModContent.ProjectileType<Destroyer_DarkBreath>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, 120);
            }
        }
        private void CreateFireWheel(Player player)
        {
            var velocity = NPC.DirectionTo(player.Center) * 3f;
            var type = ModContent.ProjectileType<Destroyer_FireWheel>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, 10);
        }
    }
}
