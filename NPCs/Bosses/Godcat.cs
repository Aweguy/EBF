using EBF.Extensions;
using EBF.Items.Magic;
using EBF.Items.Melee;
using EBF.Systems;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.NPCs.Bosses
{
    [AutoloadBossHead]
    public class Godcat : ModNPC
    {
        private bool isDodging = false;
        private bool hasDodged = false; // Used to display dodging frames
        public override string Texture => "EBF/NPCs/Bosses/Godcat_Light";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

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
            NPC.width = 34;
            NPC.height = 46;
            NPC.damage = 100;
            NPC.defense = 9999;
            NPC.lifeMax = 999999;
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
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.Godcat")
            ]);
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; //Prevent ignoring boss attacks by taking damage from other sources.
            return true;
        }
        public override void FindFrame(int frameHeight)
        {
            // Dodging frames
            if (hasDodged)
            {
                NPC.frame.Y = Main.rand.Next(6, 8) * frameHeight;
                hasDodged = false;
                return;
            }

            // Idle frames
            NPC.frameCounter += 0.1f;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type] - 2)
            {
                NPC.frameCounter = 0;
            }
            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }
        public override bool CanBeHitByNPC(NPC attacker) => !isDodging;
        public override bool? CanBeHitByProjectile(Projectile projectile) => !isDodging;
        public override void AI()
        {
            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;
            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                return;
            }

            Move(player);

            //Handle dodging
            isDodging = Main.GameUpdateCount % 60 > 10;
            if (isDodging)
            {
                DodgeOverlappingProjectile();
            }

            //Handle attacks
            if (Main.GameUpdateCount % 200 == 0)
            {
                switch (Main.rand.Next(4))
                {
                    case 0:
                        CreateJudgementWave(player);
                        break;
                    case 1:
                        //Flurry of light blade
                        NPC.ai[0] = 30;
                        break;
                    case 2:
                        CreateSeikenRing(14, 10);
                        CreateSeikenRing(8, 6);
                        CreateSeikenRing(6, 4);
                        break;
                    case 3:
                        //Cast multiple return balls
                        NPC.ai[1] = 3;
                        CreateBallArc(player, 0.66f, 6, 9f);
                        break;
                }
            }

            //Handle light blade flurry
            if (Main.GameUpdateCount % 4 == 0 && NPC.ai[0] > 0)
            {
                CreateStormSeiken(player);
                NPC.ai[0]--;
            }

            //Handle return balls
            if (Main.GameUpdateCount % 25 == 0 && NPC.ai[1] > 0)
            {
                CreateReturnBall(player, 15);
                NPC.ai[1]--;
            }
        }
        public override void OnKill()
        {
            //Let the world know the boss is dead
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedGodcat, -1);
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // The order in which you add loot will appear as such in the Bestiary. To mirror vanilla boss order:
            // 1. Trophy
            // 2. Classic Mode ("not expert")
            // 3. Expert Mode (usually just the treasure bag)
            // 4. Master Mode (relic first, pet last, everything else in between)

            // Trophy
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NeonValkTrophy>(), 10));

            // Classic Mode drops
            //npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<NanoFibre>(), 1, 3, 4));

            // Treasure bag
            //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NeonValkBossBag>()));

            // Relic
            //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.NeonValkRelic>()));

            // Pet
            //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<NeonValkPetItem>(), 4));
        }
        private void Move(Player player)
        {
            var preferredPosition = player.Center + new Vector2(-500 * NPC.direction, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.03f);
        }
        private void DodgeOverlappingProjectile()
        {
            Rectangle npcBox = NPC.Hitbox;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.friendly && !proj.minion && npcBox.Intersects(proj.Hitbox))
                {
                    hasDodged = true;
                }
            }
        }
        private void CreateJudgementWave(Player player)
        {
            // Big ol' wave of judgement lasers
            CreateJudgementAt(player.Bottom);

            for (int i = 1; i < 5; i++)
            {
                var delay = 10 * i;
                var distance = 250 * i;
                CreateJudgementAt(player.Bottom + new Vector2(distance, 0), delay);
                CreateJudgementAt(player.Bottom + new Vector2(-distance, 0), delay);
            }
        }
        private void CreateJudgementAt(Vector2 position)
        {
            var type = ModContent.ProjectileType<Seraphim_Judgement>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position.ToGroundPosition().ToSurfacePosition(), Vector2.Zero, type, NPC.damage, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.friendly = false;
            proj.hostile = true;
        }
        private void CreateJudgementAt(Vector2 position, int delay)
        {
            var type = ModContent.ProjectileType<DelayedProjectile>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), position.ToGroundPosition().ToSurfacePosition(), Vector2.Zero, type, NPC.damage, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.ai[0] = ModContent.ProjectileType<Seraphim_Judgement>();
            proj.timeLeft = delay;
            proj.friendly = false;
            proj.hostile = true;
        }
        private void CreateStormSeiken(Player player)
        {
            var position = NPC.Center + Main.rand.NextVector2CircularEdge(64, 64);
            var velocity = Vector2.Normalize(player.Center - position) * 14f;
            var type = ModContent.ProjectileType<Godcat_LightBlade>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity.RotatedByRandom(0.33f), type, NPC.damage, 3f, -1, NPC.target);
        }
        private void CreateSeikenRing(int amount, int speed)
        {
            for (float theta = 0; theta < MathF.Tau; theta += MathF.Tau / amount)
            {
                var type = ModContent.ProjectileType<Godcat_LightBlade>();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(theta) * speed, type, NPC.damage, 3f, -1, NPC.target);
            }
        }
        private void CreateReturnBall(Player player, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_LightReturnBall>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * speed, type, NPC.damage, 3f, -1, NPC.whoAmI);
        }
        private void CreateBallArc(Player player, float spread, int amount, float speed)
        {
            var type = ModContent.ProjectileType<Godcat_LightBall>();
            for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
            {
                var velocity = NPC.DirectionTo(player.Center).RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, NPC.damage, 3f);
            }
        }
    }
    /// <summary>
    /// This is a projectile that delays the spawning of another projectile.
    /// It saves us the trouble of managing timers on the spawning actor.
    /// </summary>
    public class DelayedProjectile : ModProjectile
    {
        private bool friendly;
        private bool hostile;
        public override bool ShouldUpdatePosition() => false;
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible
        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.timeLeft = 60; // 1 second delay by default
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            // Save the friendly and hostile flags so we can restore to the real projectile
            // Not done in OnSpawn, because that would set the flags before we modify the created projectile from the actor
            if (Projectile.frameCounter++ == 0)
            {
                friendly = Projectile.friendly;
                hostile = Projectile.hostile;
                Projectile.friendly = false;
                Projectile.hostile = false;
            }

            if (Projectile.timeLeft <= 1)
            {
                var type = (int)Projectile.ai[0];
                var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                proj.friendly = friendly;
                proj.hostile = hostile;

                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor) => false; // Don't draw this 
    }

    /// <summary>
    /// Variation of Light Blade that behaves differently to the one casted by heaven's gate.
    /// </summary>
    public class Godcat_LightBlade : ModProjectile
    {
        private const float turnSpeed = 0.5f;
        private bool animate = true;
        private float speed;
        private Player Target => Main.player[(int)Projectile.ai[0]];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 240;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override string Texture => "EBF/Items/Melee/HeavensGate_LightBlade";
        public override bool? CanHitNPC(NPC target) => false;
        public override bool? CanDamage() => !animate;
        public override bool ShouldUpdatePosition() => !animate;
        public override void OnSpawn(IEntitySource source)
        {
            speed = Projectile.velocity.Length();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void AI()
        {
            if (animate)
            {
                Animate();
            }
            else if (Projectile.timeLeft < 60)
            {
                animate = true;
            }
            else if (Target != null)
            {
                speed *= 1.01f;

                //Slight homing behavior while flying
                var angleToTarget = Projectile.AngleTo(Target.Center) + MathHelper.PiOver2;
                Projectile.rotation = ProjectileExtensions.SlowRotation(Projectile.rotation, angleToTarget, turnSpeed);
                Projectile.velocity = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * speed;
            }

            //Create dust
            if (Main.rand.NextBool(3))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            var velocity = Vector2.Normalize(Projectile.oldVelocity);
            var position = Projectile.position + velocity * 16f;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(position, Projectile.width, Projectile.height, DustID.AncientLight, velocity.X, velocity.Y);
                dust.noGravity = true;
            }
        }
        private void Animate()
        {
            //Advance frames every third tick
            if (Main.GameUpdateCount % 3 != 0)
                return;

            Projectile.frame++;
            switch (Projectile.frame)
            {
                case 4:
                    animate = false;
                    Projectile.netUpdate = true;
                    break;
                case 11:
                    Projectile.Kill();
                    break;
            }
        }
    }

    /// <summary>
    /// A large projectile which flies in a straight path, and explodes into smaller projectiles that home toward the caster
    /// </summary>
    public class Godcat_LightReturnBall : ModProjectile
    {
        private bool IsMiniVariant => Projectile.ai[1] == 1;
        private NPC Owner => Main.npc[(int)Projectile.ai[0]]; //Projectile.owner must be -1, otherwise it doesn't deal damage to the player. So we will pass the owner through ai
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void OnSpawn(IEntitySource source)
        {
            if (IsMiniVariant)
            {
                Projectile.scale = 0.8f;
                Projectile.timeLeft = 200;
            }
            else
            {
                Projectile.scale = 1.2f;
            }
        }
        public override void AI()
        {
            if (IsMiniVariant)
            {
                if (Main.rand.NextBool(2))
                {
                    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                    dust.noGravity = true;
                }

                Projectile.HomeTowards(Owner, 0.3f);
                if(Projectile.Distance(Owner.Center) < 32)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (!IsMiniVariant)
            {
                //Replace projectile with a bunch of mini variants, with a bit of angle variation
                var amount = 8;
                var spread = 1f;
                for (float theta = -spread; theta < spread; theta += 2 * spread / amount)
                {
                    var velocity = Projectile.velocity.RotatedBy(theta) * Main.rand.NextFloat(0.9f, 1.1f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.position, velocity, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Owner.whoAmI, 1);
                }
            }

            //Create dust
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
            }
        }
    }

    /// <summary>
    /// Basic ball projectile that flies forward
    /// </summary>
    public class Godcat_LightBall : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 360;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
                dust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight);
            }
        }
    }
}
