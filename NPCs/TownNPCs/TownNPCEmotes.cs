using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace EBF.NPCs.TownNPCs
{
    public abstract class TownNPCEmotes : ModEmoteBubble
    {
        public override string Texture => "EBF/NPCs/TownNPCs/TownNPCEmotes";
        public override void SetStaticDefaults()
        {
            AddToCategory(EmoteID.Category.Town);
        }
        
        //The size of the rectangle is very finnicky. I don't think the numbers can be adjusted without breaking where the sprite is drawn inside the emote bubble.
        public override Rectangle? GetFrame() => new(EmoteBubble.frame * 34, 28 * Row, 34, 28);
        public override Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) => new(frame * 34, 28 * Row, 34, 28);
        public virtual int Row => 0; //All our town npc emotes are stored in one spritesheet, this chooses which npc to show.
    }

    public class MattEmote : TownNPCEmotes
    {
        public override int Row => 0;
    }
    public class NatalieEmote : TownNPCEmotes
    {
        public override int Row => 1;
    }
    public class LanceEmote : TownNPCEmotes
    {
        public override int Row => 2;
    }
    public class AnnaEmote : TownNPCEmotes
    {
        public override int Row => 3;
    }
    public class NoLegsEmote : TownNPCEmotes
    {
        public override int Row => 4;
    }
}
