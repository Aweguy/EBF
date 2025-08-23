using Terraria.ID;
using Terraria.ObjectData;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;

namespace EBF.Tiles.Furniture
{
    public class GodcatAltarTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2); // start from 3x2 template
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16]; // 5 rows
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 200, 200));

            DustType = DustID.Marble;
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;

            int[] slotWithGem = [0, 0, 0];

            // Find required items
            bool requirementFulfilled = false;
            for (int slot = 0; slot < player.inventory.Length; slot++)
            {
                if (player.inventory[slot].type == ItemID.LargeRuby && player.inventory[slot].stack > 0)
                    slotWithGem[0] = slot;

                if (player.inventory[slot].type == ItemID.LargeEmerald && player.inventory[slot].stack > 0)
                    slotWithGem[1] = slot;

                if (player.inventory[slot].type == ItemID.LargeSapphire && player.inventory[slot].stack > 0)
                    slotWithGem[2] = slot;

                if (!slotWithGem.Contains(0))
                {
                    requirementFulfilled = true;
                    break;
                }
            }

            if (requirementFulfilled)
            {
                ConsumeItem(player, slotWithGem[0]);
                ConsumeItem(player, slotWithGem[1]);
                ConsumeItem(player, slotWithGem[2]);

                // Summon boss
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int npcID = ModContent.NPCType<NPCs.Bosses.Godcat.Godcat_Light>();
                    NPC.SpawnOnPlayer(player.whoAmI, npcID);
                }
            }
            else
            {
                // No item found, show warning
                CombatText.NewText(new Rectangle(i * 16, j * 16 - 20, 1, 1), Color.Red, "Missing Gems!");
                return true;
            }

            return true;
        }

        private static void ConsumeItem(Player player, int slot)
        {
            player.inventory[slot].stack--;
            if (player.inventory[slot].stack <= 0)
                player.inventory[slot].TurnToAir();
        }
    }
}
