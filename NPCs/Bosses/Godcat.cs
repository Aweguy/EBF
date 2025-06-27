using EBF.Extensions;
using EBF.Items.Magic;
using EBF.Systems;
using Microsoft.Xna.Framework;
using System;
using Terraria;
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
            if (Main.GameUpdateCount % 360 == 0)
            {
                switch (Main.rand.Next(2))
                {
                    case 0:
                        CreateJudgementStream(player);
                        break;
                    case 1:
                        CreateJudgementWave(player);
                        break;
                }
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
            var preferredPosition = player.Center + new Vector2(-300 * NPC.direction, -100);
            NPC.position = Vector2.Lerp(NPC.position, preferredPosition, 0.025f);
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
        private void CreateJudgementStream(Player player)
        {
            var dir = player.velocity.X != 0 ? Math.Sign(player.velocity.X) : NPC.direction; // If player is not moving, use NPC direction
            dir *= 100;
            for (int i = 0; i < 5; i++)
            {
                CreateJudgementAt(player.Bottom + new Vector2(dir * i, 0), delay: 10 * i);
            }
        }
        private void CreateJudgementAt(Vector2 position)
        {
            var type = ModContent.ProjectileType<Seraphim_Judgement>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), VectorUtils.GetGroundPosition(position), Vector2.Zero, type, NPC.damage, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.friendly = false;
            proj.hostile = true;
        }
        private void CreateJudgementAt(Vector2 position, int delay)
        {
            var type = ModContent.ProjectileType<DelayedProjectile>();
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), VectorUtils.GetGroundPosition(position), Vector2.Zero, type, NPC.damage, 3f, 255); //Do not ask me why owner is 255, the projectile disappears otherwise
            proj.ai[0] = ModContent.ProjectileType<Seraphim_Judgement>();
            proj.timeLeft = delay;
            proj.friendly = false;
            proj.hostile = true;
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
}
