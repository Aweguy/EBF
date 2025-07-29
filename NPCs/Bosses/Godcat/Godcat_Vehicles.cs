using EBF.EbfUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.CameraModifiers;
using System.Linq;

namespace EBF.NPCs.Bosses.Godcat
{
    public abstract class Godcat_Vehicle : ModNPC
    {
        //Textures
        protected Texture2D currentTexture;
        protected Asset<Texture2D> idleTexture;
        protected Asset<Texture2D> attackTexture;
        protected float animationSpeed = 0.1f;

        //AI
        private bool hasSearchedForOther = false; // We search for the other vehicle in AI, because OnSpawn is called before both vehicles are done initializing.
        protected NPC otherVehicle = null; // Is used to reduce aggression when both vehicles are active, and is also used to change phase only once both are dead.
        protected enum State : byte { Idle, TurningBallCircle, TurningBallSpiral, CreatorThunderBall, CreatorHolyDeathray, LimitBreak, DestroyerBallBurst, DestroyerBreath, DestroyerHomingBall, DestroyerFireWheel }
        protected State currentState = State.Idle;
        protected Dictionary<State, int> stateDurations;
        protected bool IsAlone => otherVehicle == null || !otherVehicle.active;
        protected ref float StateTimer => ref NPC.localAI[0];
        protected ref float Phase => ref NPC.ai[0];
        protected const int PunishingDistance = 810; // Used to spawn deadly projectiles if the target player is too far away.

        public override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetDefaults()
        {
            NPC.damage = 100;
            NPC.defense = 50;
            NPC.lifeMax = 300000;
            NPC.noGravity = true;

            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f; // Use all spawn slots to prevent random NPCs from spawning

            NPC.lavaImmune = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Fallen_Blood");
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; //Prevent ignoring boss attacks by taking damage from other sources.
            return true;
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }
        public override void FindFrame(int frameHeight)
        {
            // Idle frames
            NPC.frameCounter += animationSpeed;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type])
            {
                if (currentTexture != idleTexture.Value)
                    SetAnimation(idleTexture, 6);
                else
                    NPC.frameCounter = 0;
            }
            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Spawn with half health in second phase
            if (Phase == 1)
            {
                NPC.life = NPC.lifeMax / 2;
            }
        }
        public override void AI()
        {
            // Locate other vehicle when both are alive at once
            if (Phase != 0 && !hasSearchedForOther && TryFindOtherVehicle(out NPC otherVehicle))
            {
                this.otherVehicle = otherVehicle;
                hasSearchedForOther = true;
            }

            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;
            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                return;
            }

            // In first phase, leave at half health
            if (Phase == 0 && NPC.life <= NPC.lifeMax / 2)
            {
                BeginNextPhase(player);
                NPC.active = false;
                return;
            }

            Move(player);
            HandleStateChange();
            PunishFleeingPlayer(player);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!idleTexture.IsLoaded)
                return false;

            var position = NPC.Center - screenPos;
            var origin = NPC.Size * 0.5f;
            var flipX = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(currentTexture, position, NPC.frame, drawColor, 0, origin, 1, flipX, 0);
            return false;
        }
        public override void OnKill()
        {
            //Go to next phase if both are dead
            if (IsAlone)
            {
                var pos = Main.player[NPC.target].position.ToPoint() + new Point(-NPC.direction * 1600, 0);
                var type = ModContent.NPCType<Godcat_Light>();
                NPC.NewNPC(NPC.GetSource_Death(), pos.X, pos.Y, type, 0, 2);

                var pos2 = Main.player[NPC.target].position.ToPoint() + new Point(-NPC.direction * 1600, 0);
                var type2 = ModContent.NPCType<Godcat_Dark>();
                NPC.NewNPC(NPC.GetSource_Death(), pos2.X, pos2.Y, type2, 0, 2);
            }

            // Screen shake
            var modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
            Main.instance.CameraModifiers.Add(modifier);
        }
        protected abstract void Move(Player player);
        protected abstract void BeginNextPhase(Player player);
        protected void SetAnimation(Asset<Texture2D> textureAsset, int fps)
        {
            if (textureAsset == null || !textureAsset.IsLoaded)
                return;

            currentTexture = textureAsset.Value;
            animationSpeed = fps / 60f;
            NPC.frameCounter = 0;
        }
        private void HandleStateChange()
        {
            StateTimer++;
            if (StateTimer >= stateDurations[currentState])
            {
                StateTimer = 0;
                var index = Main.rand.Next(1, stateDurations.Count);
                currentState = currentState == State.Idle ? stateDurations.ElementAt(index).Key : State.Idle;
            }
        }
        private bool TryFindOtherVehicle(out NPC otherVehicle)
        {
            foreach (var npc in Main.npc)
            {
                if (npc.active && npc.type != NPC.type && npc.ModNPC is Godcat_Vehicle)
                {
                    otherVehicle = npc;
                    return true;
                }
            }

            otherVehicle = null;
            return false;
        }
        private void PunishFleeingPlayer(Player player)
        {
            if (currentState != State.Idle && NPC.Distance(player.Center) > PunishingDistance)
            {
                var position = NPC.Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 64;
                var velocity = NPC.DirectionTo(player.Center).RotatedByRandom(1f) * 20f;

                int type = 0;
                if (NPC.ModNPC is Godcat_Destroyer)
                    type = ModContent.ProjectileType<Godcat_DarkBlade>();

                else if (NPC.ModNPC is Godcat_Creator)
                    type = ModContent.ProjectileType<Godcat_LightBlade>();

                Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, type, NPC.damage, 3f);

                if (Main.GameUpdateCount % 5 == 0)
                    SoundEngine.PlaySound(SoundID.Item72, NPC.position); //Shadowbeam sound
            }
        }
    }

    [AutoloadBossHead]
    public class Godcat_Creator : Godcat_Vehicle
    {
        private Vector2 BarrelPos => NPC.Center + new Vector2(80 * NPC.direction, -16);
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 1;

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
            NPC.width = 202;
            NPC.height = 154;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Item14;
            idleTexture = ModContent.Request<Texture2D>(Texture);
            currentTexture = idleTexture.Value;

            stateDurations = new()
            {
                [State.Idle] = 200,
                [State.TurningBallCircle] = 240,
                [State.TurningBallSpiral] = 300,
                [State.CreatorThunderBall] = 200,
                [State.CreatorHolyDeathray] = 200,
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat_Creator")
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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.LightBig, -0.005f);

                    if (IsAlone)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity * 0.9f, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.LightBig, 0.005f);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item39, NPC.position); //Razorpine sound
            }
        }
        private void CreateTurningBallSpiral()
        {
            if (Main.GameUpdateCount % 2 == 0)
            {
                var speed = 4;
                var velocity = (Main.GameUpdateCount * 0.2f).ToRotationVector2() * speed;
                var type = ModContent.ProjectileType<Godcat_TurningBall>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, (float)GodcatBallTypes.LightBig, -0.005f);

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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f);
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
                var amount = 24;
                if (!IsAlone)
                {
                    amount = 18;
                }

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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, delay, drawBehind, NPC.whoAmI);
                    delay += 2;
                }
            }

            //Final huge burst
            else if (StateTimer == 199)
            {
                SoundEngine.PlaySound(SoundID.Item8, NPC.position);

                //Form a ring of thunder balls
                var amount = 8;
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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, delay, drawBehind, NPC.whoAmI);
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
                ChargeUpDust();
            }
            else if (StateTimer == 150)
            {
                ShootLaser();

                //Create arc of diamonds
                var speed = 4f;
                var amount = 4;
                var spread = 1f;
                var type = ModContent.ProjectileType<Godcat_LightDiamond>();
                var dir = (NPC.direction == 1 ? 0 : MathHelper.Pi).ToRotationVector2();
                for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
                {
                    var velocity = dir.RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f);
                }
            }
        }
        private void ShootLaser()
        {
            var velocity = new Vector2(NPC.direction, 0);
            var type = ModContent.ProjectileType<Creator_HolyDeathray>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3, -1, NPC.whoAmI);

            var sound = SoundID.Zombie104; // Moon Lord deathray sound
            sound.Pitch = 1.4f;
            sound.Volume = 0.3f;
            SoundEngine.PlaySound(sound, NPC.Center);
        }
        private void ChargeUpDust()
        {
            var pos = BarrelPos + new Vector2(NPC.direction, 0).RotatedByRandom(0.5f) * 32;
            var vel = pos.DirectionTo(BarrelPos);
            var dust = Dust.NewDustPerfect(pos, DustID.AncientLight, vel, 0, default, 2.0f);
            dust.noGravity = true;
        }
    }

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
            else if (StateTimer > windupTime && Main.GameUpdateCount % 15 == 0)
            {
                var speed = 7f * Main.rand.NextFloat(0.8f, 1.2f);
                var velocity = NPC.DirectionTo(player.Center).RotatedByRandom(0.5f) * speed + player.velocity * 0.5f;
                var type = ModContent.ProjectileType<Destroyer_DarkBreath>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f, -1, 80);
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
