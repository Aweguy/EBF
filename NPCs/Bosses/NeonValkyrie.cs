using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.Utilities;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.CameraModifiers;
using Terraria.GameContent.ItemDropRules;
using ReLogic.Content;
using EBF.Dusts;
using EBF.Systems;
using EBF.EbfUtils;
using EBF.NPCs.Machines;
using EBF.Items.Materials;
using EBF.Items.TreasureBags;
using EBF.Items.Placeables.Furniture.BossTrophies;

namespace EBF.NPCs.Bosses
{
    [AutoloadBossHead]
    public class NeonValkyrie : ModNPC
    {
        #region Fields & Properties


        //Movement
        private const int hoverDistance = 48;
        private float horizontalAcceleration = 0.2f;
        private float horizontalMaxSpeed = 6f;
        private Vector2 groundPos;
        private ref float JumpCooldown => ref NPC.localAI[1];

        //AI
        private enum State : byte { Move, Shoot, SummonFlybots, SummonTurret, RevUp }
        private State currentState = State.Move;
        private readonly Dictionary<State, int> stateDurations = new()
        {
            [State.Move] = 200,
            [State.Shoot] = 60,
            [State.SummonFlybots] = 40,
            [State.SummonTurret] = 1,
            [State.RevUp] = 140
        };
        private readonly WeightedRandom<State> weightedRandom = new();
        private ref float StateTimer => ref NPC.localAI[0];
        private ref float InSecondPhase => ref NPC.ai[0];

        //Attachment
        private NPC attachedNPC;
        private const int minimumAttachmentDowntime = 900;
        private bool HasAttachment => attachedNPC != null && attachedNPC.active;
        private bool IsAttachmentShooting => HasAttachment && attachedNPC.ai[0] == 1;
        private Vector2 AttachmentBasePos => NPC.Center + new Vector2(32 * -NPC.spriteDirection, -20);
        private ref float TimePassedWithoutAttachment => ref NPC.localAI[2];

        //Other
        private Asset<Texture2D> glowTexture;
        private Vector2 GunTipPos => NPC.Center + new Vector2(75 * NPC.spriteDirection, -16);


        #endregion Fields & Properties

        public override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            //Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type); //Grouped with other bosses
            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "EBF/Assets/Textures/Bestiary/NeonValkyrie_Preview",
                PortraitScale = 0.6f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            NPC.width = 300;
            NPC.height = 64;
            NPC.damage = 50;
            NPC.defense = 20;
            NPC.lifeMax = 40000;

            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f; // Use all spawn slots to prevent random NPCs from spawning

            NPC.lavaImmune = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/M3CHANICAL_C0N-D4MNATION");
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");

            //Add the chances for each state
            weightedRandom.Add(State.Shoot, 2.0f);
            weightedRandom.Add(State.SummonFlybots, 0.5f);
            weightedRandom.Add(State.SummonTurret, 1.0f);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
				new FlavorTextBestiaryInfoElement("Mods.EBF.Bestiary.NeonValkyrie")
            ]);
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; //Prevent ignoring boss attacks by taking damage from other sources.
            return true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.TargetClosest(false);
            groundPos = NPC.position.ToGroundPosition();
        }
        public override void AI()
        {
            JumpCooldown--;
            NPC.TargetClosest();
            NPC.spriteDirection = NPC.direction;
            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10); // Despawns in 10 ticks
                if (HasAttachment)
                    attachedNPC.StrikeInstantKill();
                return;
            }

            if (InSecondPhase == 1)
            {
                SecondStageSmokeEffect();
            }

            if (HasAttachment)
            {
                NPC.defense = 70;
                TimePassedWithoutAttachment = 0;
                attachedNPC.Bottom = AttachmentBasePos;
                attachedNPC.velocity = NPC.velocity;

                //Enrage attached turret
                if (InSecondPhase == 1)
                    attachedNPC.ai[1] = 1;
            }
            else
            {
                NPC.defense = 20;
                TimePassedWithoutAttachment++;
            }

            Hover(player);
            SpawnHoverRings();

            switch (currentState)
            {
                case State.Move when !IsAttachmentShooting:
                    Move(player);
                    break;
                case State.Shoot:
                    Shoot(player);
                    break;
                case State.SummonFlybots when !HasAttachment:
                    SummonFlybots();
                    break;
                case State.SummonTurret when TimePassedWithoutAttachment > minimumAttachmentDowntime:
                    SummonAttachment();
                    break;
                case State.RevUp:
                    TransitionToSecondStage();
                    break;
            }

            HandleStateChanging();
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!glowTexture.IsLoaded)
                return;

            var position = NPC.Center - screenPos + new Vector2(0, 4);
            var origin = glowTexture.Value.Size() * 0.5f;
            var flip = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            //Pulse color
            var pulse = (float)Math.Abs(Math.Sin(Main.time * 0.02f));
            var color = new Color(pulse, pulse, pulse);

            spriteBatch.Draw(glowTexture.Value, position, null, color, 0f, origin, 1f, flip, 0f);
        }
        public override void OnKill()
        {
            if (HasAttachment)
                attachedNPC.StrikeInstantKill();

            NPC.CreateExplosionEffect(EBFUtils.ExplosionSize.Large);

            var sound = SoundID.NPCDeath37; //37, 56
            sound.Pitch = -1f;
            sound.Volume = 1.0f;
            SoundEngine.PlaySound(sound, NPC.position);

            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore0").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore0").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore1").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore2").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore3").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore3").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore4").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore4").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore5").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, (-Vector2.UnitY * 10).RotatedByRandom(2f) + NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore5").Type, NPC.scale);

            // Screen shake
            var modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
            Main.instance.CameraModifiers.Add(modifier);

            //Let the world know the boss is dead
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedNeonValk, -1); 
        }
        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // The order in which you add loot will appear as such in the Bestiary. To mirror vanilla boss order:
            // 1. Trophy
            // 2. Classic Mode ("not expert")
            // 3. Expert Mode (usually just the treasure bag)
            // 4. Master Mode (relic first, pet last, everything else in between)

            // Trophy
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NeonValkTrophy>(), 10));

            // Classic Mode drops
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<NanoFibre>(), 1, 3, 4));
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<RamChip>(), 1, 6, 8));
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ItemID.ExplosivePowder, 1, 28, 32));

            // Treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NeonValkBossBag>()));

            // Relic
            //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.NeonValkRelic>()));

            // Pet
            //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<NeonValkPetItem>(), 4));
        }
        private void HandleStateChanging()
        {
            StateTimer++;
            if (StateTimer >= stateDurations[currentState])
            {
                StateTimer = 0;
                currentState = currentState != State.Move ? State.Move : weightedRandom.Get();

                //Slightly randomize timing to make it more interesting
                if (currentState != State.SummonTurret)
                {
                    StateTimer -= Main.rand.Next(40);
                }

                var sound = Main.rand.NextBool(2) ? SoundID.Zombie48 : SoundID.Zombie49; //deadly sphere idle
                sound.Volume = 2.0f;
                SoundEngine.PlaySound(sound, NPC.position);
            }

            //Go into 2nd phase
            if (InSecondPhase == 0 && NPC.life < NPC.lifeMax / 2)
            {
                StateTimer = 0;
                currentState = State.RevUp;
            }
        }
        private void Hover(Player player)
        {
            //Skip ground search most frames cuz it is expensive
            if (Main.GameUpdateCount % 10 == 0)
            {
                if (NPC.BottomLeft.Y < player.position.Y && NPC.DirectionTo(player.position).Y > 0.4f)
                {
                    groundPos = player.BottomLeft.ToGroundPosition();
                    return; //No need to hover yet
                }
                else
                {
                    bool foundGround = EBFUtils.TryGetGroundPosition(NPC.BottomLeft, new Vector2(NPC.width, hoverDistance * 2), out Vector2 ground, true);
                    groundPos = foundGround ? ground : groundPos + new Vector2(0, hoverDistance * 2);
                }
            }

            float dist = groundPos.Y - NPC.BottomLeft.Y;
            if (dist >= -16 && dist < hoverDistance) // -16 because found ground can be inside Neon Valk and result in small negative values, limited to tile size.
            {
                //Hard brake if very close and falling
                if (dist < hoverDistance / 2 && NPC.velocity.Y > 0)
                {
                    NPC.velocity.Y *= 0.2f;
                }

                //Add force up
                NPC.velocity.Y -= (hoverDistance - dist) * 0.05f;
                if (Math.Abs(NPC.velocity.Y) > 1)
                {
                    NPC.velocity.Y *= 0.9f;
                }
            }
        }
        private void SpawnHoverRings()
        {
            if(Main.GameUpdateCount % 20 == 0)
            {
                var type = ModContent.DustType<HoverRing>();
                var velocity = new Vector2(NPC.velocity.X / 2, 2);
                Dust.NewDustPerfect(NPC.Bottom + new Vector2(60, 0), type, velocity);
                Dust.NewDustPerfect(NPC.Bottom - new Vector2(90, 0), type, velocity);
            }
        }
        private void Move(Player player)
        {
            NPC.velocity.X += NPC.DirectionTo(player.position).X * horizontalAcceleration;
            NPC.velocity.X = Math.Clamp(NPC.velocity.X, -horizontalMaxSpeed, horizontalMaxSpeed);

            //if near ground AND below player AND angle to player is over 45 degrees.
            if (groundPos.Y - NPC.BottomLeft.Y < hoverDistance && NPC.position.Y > player.BottomLeft.Y && NPC.position.DirectionTo(player.position).Y < -0.5f)
                Jump(player);
        }
        private void Jump(Player player)
        {
            if (JumpCooldown > 0 || IsAttachmentShooting)
                return;

            JumpCooldown = 120;

            float distanceY = player.position.Y - NPC.BottomLeft.Y;
            float jumpVelocityY = (float)Math.Sqrt(2 * NPC.gravity * Math.Abs(distanceY));
            NPC.velocity.Y = -jumpVelocityY;
        }
        private void Shoot(Player player)
        {
            NPC.velocity.X *= 0.95f;

            if (Main.GameUpdateCount % 3 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item11, NPC.position);

                var velocity = GunTipPos.DirectionTo(player.Center) * 10;
                Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), GunTipPos, velocity.RotatedByRandom(0.1f), ProjectileID.Bullet, NPC.damage / 2, 3);
                proj.friendly = false;
                proj.hostile = true;
            }
        }
        private void SummonFlybots()
        {
            NPC.velocity.X *= 0.95f;

            if (Main.GameUpdateCount % 15 == 0)
            {
                //Spawn bot
                var pos = AttachmentBasePos.ToPoint();
                var type = Main.rand.NextBool(2) ? ModContent.NPCType<RedFlybot>() : ModContent.NPCType<BlueFlybot>();
                var npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), pos.X, pos.Y, type);
                npc.velocity.Y = -5;

                //Extra flair
                SoundEngine.PlaySound(SoundID.Item113, NPC.position);
                for (int i = 0; i < 10; i++)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.RedTorch);
            }
        }
        private void SummonAttachment()
        {
            //Determine attachment
            int type;
            if(InSecondPhase == 1)
                type = Main.rand.NextBool(2) ? ModContent.NPCType<LaserTurret>() : ModContent.NPCType<NukeStand>();
            else 
                type = Main.rand.NextBool(2) ? ModContent.NPCType<HarpoonTurret>() : ModContent.NPCType<CannonTurret>();

            //Add attachment to NV
            attachedNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), 0, 0, type, 0, 0, InSecondPhase);
            attachedNPC.Bottom = AttachmentBasePos;
        }
        private void TransitionToSecondStage()
        {
            NPC.velocity.X *= 0.9f;
            InSecondPhase = 1;

            if (StateTimer < 30) //Give some time to brake.
                return;

            if (StateTimer == 30)
            {
                var revSound = new SoundStyle("EBF/Assets/Sfx/NeonValkyrie_RevUp") { Volume = 0.5f };
                SoundEngine.PlaySound(revSound, NPC.position);
            }
            else if (StateTimer >= 139)
            {
                //Adjust stats
                NPC.color = new Color(255, 200, 200);
                stateDurations[State.Move] = 160;
                stateDurations[State.Shoot] = 70;
                stateDurations[State.SummonFlybots] = 50;
                horizontalAcceleration *= 1.5f;
                horizontalMaxSpeed *= 1.25f;
            }

            //Color effect (increases, then decreases)
            var midPoint = (stateDurations[State.RevUp] - 30) / 2;
            var percentage = (StateTimer - 30) / midPoint;
            if (percentage > 1) percentage = 1 - (StateTimer - 90) / midPoint;
            NPC.color = Color.Lerp(Color.White, Color.Red, percentage);

            //Shake
            var intensity = 8;
            var dir = StateTimer % 2 == 0 ? 1 : -1;
            NPC.position += new Vector2(dir * percentage * intensity, 0);

            //Smoke effect
            var pos = NPC.position + Main.rand.NextVector2FromRectangle(NPC.frame);
            var gore = Gore.NewGoreDirect(NPC.GetSource_FromAI(), pos, -Vector2.UnitY, GoreID.Smoke1 + Main.rand.Next(3));
            gore.alpha = 125;
        }
        private void SecondStageSmokeEffect()
        {
            if (Main.GameUpdateCount % 10 == 0)
            {
                var pos = NPC.position + Main.rand.NextVector2FromRectangle(NPC.frame);
                var gore = Gore.NewGoreDirect(NPC.GetSource_FromAI(), pos, -Vector2.UnitY, GoreID.Smoke1 + Main.rand.Next(3));
                gore.alpha = 185;
            }
        }
    }
}
