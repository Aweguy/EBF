using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.DataStructures;

namespace EBF.NPCs.Bosses.Godcat
{
    [AutoloadBossHead]
    public class Godcat_Dark : Godcat
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/Godcat_Dark_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            stateDurations = new()
            {
                [State.Idle] = Main.expertMode ? 180 : 220,
                [State.GoingTowardsGround] = 9999999,
                [State.InGround] = 9999999,
                [State.SeikenStorm] = 120,
                [State.SeikenRing] = 1,
                [State.DarkReturnBall] = 70,
                [State.DarkBallStream] = 120,
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Dark")
            ]);
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Dialogue
            string text = Phase switch
            {
                0 => "This world began in fire, and it shall end in fire!",
                1 => "Foolish, I am far beyond you.",
                2 => "To craft such brilliant weapons of war… May we meet again!",
                _ => ""
            };

            Main.NewText(text, Color.Red);
        }
        protected override void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(-550, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.05f);
        }
        protected override void HandleAttacks(Player player)
        {
            //Don't attack in final phase
            if (Phase == 2)
            {
                return;
            }

            switch (currentState)
            {
                case State.Idle:
                    break;

                case State.SeikenStorm:
                    if (StateTimer % 4 == 0)
                        CreateDarkStormSeiken(player);
                    break;

                case State.SeikenRing:
                    CreateDarkSeikenRing(14, 10);
                    CreateDarkSeikenRing(8, 6);
                    CreateDarkSeikenRing(6, 4);
                    break;

                case State.DarkReturnBall:
                    if (StateTimer % 30 == 0)
                    {
                        CreateDarkReturnBall(player, 15);

                        if (StateTimer == 0)
                            CreateDarkBallArc(player, 0.66f, 6, 9f);
                    }
                    break;

                case State.DarkBallStream:
                    if (StateTimer % 2 == 0)
                    {
                        CreateDarkStreamBall(player, 6f, 0.1f);
                    }
                    if (StateTimer % 40 == 0)
                    {
                        CreateDarkBallArc(player, 1f, 5, 7f);
                        SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
                    }
                    break;
            }
        }
        protected override void SummonVehicle(Player player)
        {
            var pos = player.position.ToPoint() + new Point(-NPC.direction * 1600, 0);
            var type = ModContent.NPCType<Godcat_Destroyer>();
            NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type, 0, Phase);

            type = ModContent.NPCType<RedCrystal>();
            var amount = Phase == 0 ? 2 : 1;
            for (var i = 0; i < amount; i++)
                NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type);

            //Dialogue
            var text = Phase == 0 ? "You cannot escape my wrath." : "No one who dares spite me can be permitted to stand!";
            Main.NewText(text, Color.Red);
        }
        protected override void SpawnDust()
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
            }
        }
        private void CreateDarkStormSeiken(Player player)
        {
            var position = NPC.Center + Main.rand.NextVector2CircularEdge(64, 64);
            var velocity = Vector2.Normalize(player.Center - position) * 14f;
            var type = ModContent.ProjectileType<Godcat_DarkBlade>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity.RotatedByRandom(0.33f), type, NPC.damage / 4, 3f, -1, NPC.target);

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateDarkSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_DarkBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage / 4, 3f, -1, NPC.target);
            }

            SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
        }
        private void CreateDarkStreamBall(Player player, float speed, float deviation)
        {
            var multiplier = 0.5f + (StateTimer / stateDurations[State.DarkBallStream]);
            var velocity = NPC.DirectionTo(player.Center).RotatedByRandom(deviation) * speed * multiplier + player.velocity * 0.1f;
            var type = ModContent.ProjectileType<Godcat_BallProjectile>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f, -1, (int)GodcatBallTypes.DarkBig);
            proj.scale = Main.rand.NextFloat(0.9f, 1.1f) * multiplier;

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateDarkReturnBall(Player player, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_ReturnBall>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * speed, type, NPC.damage / 4, 3f, -1, (float)GodcatBallTypes.DarkBig, NPC.whoAmI);

            SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
        }
        private void CreateDarkBallArc(Player player, float spread, int amount, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_BallProjectile>();
            for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
            {
                var velocity = NPC.DirectionTo(player.Center).RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f, -1, (float)GodcatBallTypes.DarkSmall);
            }
        }
    }
}
