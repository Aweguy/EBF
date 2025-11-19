using EBF.EbfUtils;
using EBF.Items.Placeables.Furniture.BossTrophies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses.Godcat
{
    [AutoloadBossHead]
    public class Godcat_Creator : Godcat_VehicleNPC
    {
        private Vector2 BarrelPos => NPC.Center + new Vector2(80 * NPC.direction, -16);
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 6;

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/Godcat_Creator_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 256;
            NPC.height = 200;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Item14;
            idleTexture = ModContent.Request<Texture2D>(Texture);
            currentTexture = idleTexture.Value;

            stateDurations = new()
            {
                [State.Idle] = Main.expertMode ? 180 : 220,
                [State.TurningBallCircle] = 240,
                [State.TurningBallSpiral] = 300,
                [State.CreatorThunderBall] = 200,
                [State.CreatorHolyDeathray] = 200,
            };

            attackManager
                .Add(1, 1f)
                .Add(2, 1f)
                .Add(3, 1f)
                .Add(4, 1f);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow, // Background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Creator")
            ]);
        }
        public override void AI()
        {
            base.AI();
            switch (currentState)
            {
                case State.TurningBallCircle:
                    CreateTurningBallsCircles();
                    break;
                case State.TurningBallSpiral:
                    CreateTurningBallSpiral();
                    break;
                case State.CreatorThunderBall:
                    CreateThunderBalls();
                    break;
                case State.CreatorHolyDeathray:
                    CreateHolyDeathray();
                    break;
            }
        }
        public override void OnKill()
        {
            base.OnKill();
            NPC.CreateExplosionEffect(EBFUtils.ExplosionSize.Large);

            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore0").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore1").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore2").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore3").Type, NPC.scale);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GodcatCreatorTrophy>(), 10));
        }
        protected override void Move(Player player)
        {
            var offset = new Vector2(550, -100);
            if (currentState == State.CreatorHolyDeathray)
            {
                offset = new Vector2(400, 16);
            }

            var preferredPosition = player.Center + offset;
            NPC.Center = Vector2.Lerp(NPC.Center, preferredPosition, 0.03f);
        }
        protected override void BeginNextPhase(Player player)
        {
            var type = ModContent.NPCType<Godcat_Dark>();
            var pos = player.Center.ToPoint() + new Point(NPC.direction * 1600, 0);
            NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, type, 0, Phase);
        }
        protected override void CreateHalfHealthHurtEffect()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.position);
            NPC.CreateExplosionEffect(EBFUtils.ExplosionSize.Large);
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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f, -1, (float)GodcatBallTypes.LightBig, -0.005f);

                    if (IsAlone)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity * 0.9f, type, NPC.damage / 4, 3f, -1, (float)GodcatBallTypes.LightBig, 0.005f);
                }

                SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
            }
        }
        private void CreateTurningBallSpiral()
        {
            var shootDelay = IsAlone ? 2 : 4;
            if (Main.GameUpdateCount % shootDelay == 0)
            {
                var speed = 4;
                var velocity = (Main.GameUpdateCount * 0.2f).ToRotationVector2() * speed;
                var type = ModContent.ProjectileType<Godcat_TurningBall>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f, -1, (float)GodcatBallTypes.LightBig, -0.005f);

                SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
            }

            if (IsAlone && Main.GameUpdateCount % 60 == 0)
            {
                var amount = 12;
                var speed = 6;
                var type = ModContent.ProjectileType<Godcat_LightDiamond>();
                for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
                {
                    var velocity = Vector2.UnitX.RotatedBy(theta) * speed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f);
                }

                SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
            }
        }
        private void CreateThunderBalls()
        {
            //Three small bursts
            if (StateTimer == 0 || StateTimer == 66 || StateTimer == 133)
            {
                SoundEngine.PlaySound(SoundID.Item8, NPC.position);

                //Form a ring of thunder balls
                var amount = IsAlone ? 16 : 10;
                if (Main.expertMode) amount += 4;

                var delay = 40;
                var type = ModContent.ProjectileType<Creator_Thunderball>();
                for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
                {
                    var velocity = theta.ToRotationVector2();
                    velocity.Y *= 0.4f;

                    //Some balls should draw behind creator, we send that info via ai[1]
                    var drawBehind = 0;
                    if (theta > MathF.PI) // ">" because theta goes clockwise for some reason?
                    {
                        drawBehind = 1;
                    }

                    //ai[0] is how long it takes before the balls launch
                    //ai[2] is the owner, real owner must be -1 for dmg to work, owner is used to keep balls attached until they launch
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f, -1, delay, drawBehind, NPC.whoAmI);
                    delay += 2;
                }
            }

            //Final huge burst
            else if (StateTimer == 199)
            {
                SoundEngine.PlaySound(SoundID.Item8, NPC.position);

                //Form a ring of thunder balls
                var amount = IsAlone ? 6 : 4;
                if (Main.expertMode) amount += 2;

                var delay = 40;
                var type = ModContent.ProjectileType<Creator_HugeThunderball>();
                for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
                {
                    var velocity = theta.ToRotationVector2();
                    velocity.Y *= 0.4f;

                    //Some balls should draw behind creator, we send that info via ai[1]
                    var drawBehind = 0;
                    if (theta > MathF.PI) // ">" because theta goes clockwise for some reason?
                    {
                        drawBehind = 1;
                    }

                    //ai[0] is how long it takes before the balls launch
                    //ai[2] is the owner, real owner must be -1 for dmg to work, owner is used to keep balls attached until they launch
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 2, 3f, -1, delay, drawBehind, NPC.whoAmI);
                    delay += 10;
                }
            }
        }
        private void CreateHolyDeathray()
        {
            if (StateTimer == 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath58, NPC.Center);
            }

            if (StateTimer < 150)
            {
                // Create charge-up dust
                var pos = BarrelPos + new Vector2(NPC.direction, 0).RotatedByRandom(0.5f) * 32;
                var vel = pos.DirectionTo(BarrelPos);
                var dust = Dust.NewDustPerfect(pos, DustID.AncientLight, vel, 0, default, 2.0f);
                dust.noGravity = true;
            }
            else if (StateTimer == 150)
            {
                // Shoot laser
                var velocity = new Vector2(NPC.direction, 0);
                var type = ModContent.ProjectileType<Creator_HolyDeathray>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3, -1, NPC.whoAmI);

                var sound = SoundID.Zombie104; // Moon Lord deathray sound
                sound.Pitch = 1.4f;
                sound.Volume = 0.3f;
                SoundEngine.PlaySound(sound, NPC.Center);

                // Create arc of diamonds
                if (!IsAlone)
                    return;

                var speed = 4f;
                var amount = 10;
                if (Main.expertMode)
                {
                    amount += 1;
                    speed += 1f;
                }

                var spread = 1.5f;
                type = ModContent.ProjectileType<Godcat_LightDiamond>();
                var dir = (NPC.direction == 1 ? 0 : MathHelper.Pi).ToRotationVector2();
                for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
                {
                    velocity = dir.RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage / 4, 3f);
                }
            }
        }
    }
}
