using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.CameraModifiers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using EBF.Extensions;
using Terraria.Audio;
using Terraria.DataStructures;
using EBF.NPCs.Machines;
using Terraria.Utilities;

namespace EBF.NPCs.Bosses
{
    [AutoloadBossHead]
    public class NeonValkyrie : ModNPC
    {
        private Asset<Texture2D> glowTexture;
        private WeightedRandom<int> weightedRandom = new();
        private const int hoverDistance = 48;
        private const float horizontalAcceleration = 0.2f;
        private const float horizontalMaxSpeed = 6f;
        private Vector2 groundPos;
        private int state = 0;
        private readonly int[] stateDurations = [200, 60, 40];
        private Vector2 BarrelPos => NPC.Center + new Vector2(75 * NPC.spriteDirection, -16);
        private Vector2 AttachmentBasePos => NPC.Center + new Vector2(-36 * NPC.spriteDirection, -20);
        private ref float StateTimer => ref NPC.localAI[0];
        private ref float JumpCooldown => ref NPC.localAI[1];
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
            NPC.defense = 24;
            NPC.lifeMax = 40000;

            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Roar;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f; // Use all spawn slots to prevent random NPCs from spawning

            NPC.lavaImmune = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/M3CHANICAL_C0N-D4MNATION");
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");

            //Add the chances for each state
            weightedRandom.Add(1, 2);
            weightedRandom.Add(2, 1);
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
            groundPos = VectorUtils.GetGroundPosition(Main.player[NPC.target].position);
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
                return;
            }

            Hover(player);

            switch (state)
            {
                case 0:
                    Move(player);
                    break;
                case 1:
                    Shoot(player);
                    break;
                case 2:
                    SummonFlybots();
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
            //Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), ModContent.GoreType<NeonValkyrieGore>());

            // Screen shake
            var modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
            Main.instance.CameraModifiers.Add(modifier);
        }

        private void HandleStateChanging()
        {
            StateTimer++;
            if (StateTimer >= stateDurations[state] + Main.rand.Next(20))
            {
                StateTimer = 0;
                state = state != 0 ? 0 : weightedRandom.Get();
            }
        }
        private void Hover(Player player)
        {
            //Skip ground search most frames cuz it is expensive
            if (Main.GameUpdateCount % 10 == 0)
            {
                if (NPC.BottomLeft.Y < player.position.Y)
                {
                    groundPos = player.BottomLeft;
                    return; //No need to hover yet
                }
                else
                {
                    bool foundGround = VectorUtils.GetGroundPosition(NPC.BottomLeft, new Vector2(NPC.width, hoverDistance * 2), out Vector2 ground, true);
                    groundPos = foundGround ? ground : groundPos + new Vector2(0, hoverDistance * 2);
                }
            }

            float dist = groundPos.Y - NPC.BottomLeft.Y;
            if (dist < hoverDistance)
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
            if (JumpCooldown > 0)
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

                var velocity = BarrelPos.DirectionTo(player.Center) * 10;
                Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), BarrelPos, velocity.RotatedByRandom(0.1f), ProjectileID.Bullet, NPC.damage / 2, 3);
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
                var type = ModContent.NPCType<RedFlybot>();
                var npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), pos.X, pos.Y, type);
                npc.velocity.Y = -5;

                //Extra flair
                SoundEngine.PlaySound(SoundID.Item113, NPC.position);
                for (int i = 0; i < 10; i++)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.RedTorch);
            }
        }
    }
}
