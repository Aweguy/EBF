using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace EBF.Abstract_Classes
{
    [AutoloadHead]
    public abstract class EBFTownNPC : ModNPC
    {
        #region Hooks
        public virtual void SetStaticDefaultsSafe() { }
        public virtual void SetDefaultsSafe() { }
        public virtual void HitEffectSafe(NPC.HitInfo hit) { }
        public virtual WeightedRandom<string> GetChatSafe(WeightedRandom<string> dialogue) { return dialogue; }
        #endregion Hooks


        public override void Load()
        {
            //Load heads to be used in housing banners and in the housing npc list.
            //Normal head texture is automatically loaded via [AutoloadHead]
            Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
        }
        //This adds the shimmer and party variants to the npcs.
        public sealed override ITownNPCProfile TownNPCProfile() => new Profiles.StackedNPCProfile(
            new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
            new Profiles.DefaultNPCProfile(Texture + "_Shimmer", NPCHeadLoader.GetHeadSlot(Texture + "_Shimmer_Head"), Texture + "_Shimmer_Party"));
        public sealed override void SetStaticDefaults()
        {
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.ShimmerTownTransform[Type] = true; // Makes this NPC transform when touching shimmer liquid, instead of becoming invisible.
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Velocity = 1f }; //Draws the npc walking in the bestiary.
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

            SetStaticDefaultsSafe();
        }
        public sealed override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.defense = 25;
            NPC.lifeMax = 500;
            NPC.knockBackResist = 0.8f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = NPCAIStyleID.Passive;

            SetDefaultsSafe();
        }
        public sealed override string GetChat()
        {
            if (NPC.homeless)
                return this.GetLocalizedValue("Chat.Homeless1");

            //Handle all other dialogue
            var dialogue = new WeightedRandom<string>();

            dialogue.Add(this.GetLocalizedValue("Chat.Normal1"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal2"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal3"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal4"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal5"));
            dialogue.Add(this.GetLocalizedValue("Chat.Normal6"));

            if (Main.dayTime)
                dialogue.Add(this.GetLocalizedValue("Chat.Day1"));
            else
            {
                dialogue.Add(this.GetLocalizedValue("Chat.Night1"));
                dialogue.Add(this.GetLocalizedValue("Chat.Night2"));
            }

            if (Main.raining)
            {
                dialogue.Add(this.GetLocalizedValue("Chat.Rain1"));
                dialogue.Add(this.GetLocalizedValue("Chat.Rain2"));
            }

            if (Main.IsItStorming)
            {
                dialogue.Add(this.GetLocalizedValue("Chat.Storm1"));
                dialogue.Add(this.GetLocalizedValue("Chat.Storm2"));
            }

            if (Main.IsItAHappyWindyDay)
            {
                dialogue.Add(this.GetLocalizedValue("Chat.WindyDay1"));
                dialogue.Add(this.GetLocalizedValue("Chat.WindyDay2"));
            }

            if (Main.bloodMoon)
            {
                dialogue.Add(this.GetLocalizedValue("Chat.BloodMoon1"));
                dialogue.Add(this.GetLocalizedValue("Chat.BloodMoon2"));
            }

            if (Main.hardMode)
                dialogue.Add(this.GetLocalizedValue("Chat.Hardmode1"));

            if (BirthdayParty.PartyIsUp)
                dialogue.Add(this.GetLocalizedValue("Chat.Party1"));

            if (Main.LocalPlayer.ZoneGraveyard)
            {
                dialogue.Add(this.GetLocalizedValue("Chat.Graveyard1"));
                dialogue.Add(this.GetLocalizedValue("Chat.Graveyard2"));
            }

            if (Main.LocalPlayer.ZoneCorrupt)
                dialogue.Add(this.GetLocalizedValue("Chat.Corrupt1"));

            if (Main.LocalPlayer.ZoneCrimson)
                dialogue.Add(this.GetLocalizedValue("Chat.Crimson1"));

            return GetChatSafe(dialogue);
        }
        public sealed override void HitEffect(NPC.HitInfo hit)
        {
            // Spawn more dust if killed.
            int num = NPC.life > 0 ? 1 : 5;
            for (int k = 0; k < num; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
            }

            // Create gore when the NPC is killed.
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                var position = NPC.position + (NPC.Size / 2) - Vector2.One * 24;
                Gore.NewGore(NPC.GetSource_Death(), position, Vector2.Zero, Main.rand.Next(61, 64), 1.5f);

                /** Better gore (requires gore sprites)
                // Retrieve the gore types. This NPC has shimmer and party variants for head, arm, and leg gore. (12 total gores)
                string variant = "";
                if (NPC.IsShimmerVariant) variant += "_Shimmer";
                if (NPC.altTexture == 1) variant += "_Party";
                int hatGore = NPC.GetPartyHatGore();
                int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
                int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
                int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

                // Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
                if (hatGore > 0)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
                */
            }

            HitEffectSafe(hit);
        }
    }
}
