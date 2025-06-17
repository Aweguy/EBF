using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;

namespace EBF.Tiles.Furniture.BossRelics
{
    // Common code for a Master Mode boss relic
    // Supports optional Item.placeStyle handling if you wish to add more relics but use the same tile type
    internal class NeonValkRelicTile : ModTile
    {
        public const int FrameWidth = 18 * 3;
        public const int FrameHeight = 18 * 4;
        public const int HorizontalFrames = 1;
        public const int VerticalFrames = 1; // Increase this if using the Item.placeStyle approach with multiple relics

        public Asset<Texture2D> RelicTexture;
        public virtual string RelicTextureName => "EBF/Tiles/Furniture/BossRelics/NeonValkRelicTile"; // Relic head
        public override string Texture => "EBF/Tiles/Furniture/BossRelics/RelicPedestal"; // Relic pedestal
        public override void Load() => RelicTexture = ModContent.Request<Texture2D>(RelicTextureName); // Cache the head texture
        
        public override void SetStaticDefaults()
        {
            Main.tileShine[Type] = 400; // Emits golden particles
            Main.tileFrameImportant[Type] = true; // Required for multitiles
            TileID.Sets.InteractibleByNPCs[Type] = true; // Town NPCs will palm their hand at this tile

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4); // Relics are 3x4
            TileObjectData.newTile.LavaDeath = false; // Doesn't break in lava
            TileObjectData.newTile.DrawYOffset = 2; // Sinks slightly into ground
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft; // Default player-facing direction
            TileObjectData.newTile.StyleHorizontal = false; // Alternate sprites are laid out vertically

            // Controls how styles are read from the sprite sheet
            TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.styleLineSkipVisualOverride = 0; // Forces tile preview to use first style visually

            // Register alternate layout (player placing it facing right)
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);

            // Register tile
            TileObjectData.addTile(Type);

            // Register map name and color
            AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
        }

        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            // Forces pedestal to draw even if placeStyle differs
            tileFrameX %= FrameWidth;
            tileFrameY %= FrameHeight * 2; // Two placement directions: left/right
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            // Relic doesn't include the hovering part on its sheet, so we manually register a special draw point
            if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
            {
                Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Ensure tile exists before drawing
            Point p = new(i, j);
            Tile tile = Main.tile[p.X, p.Y];
            if (!tile.HasTile)
                return;

            Texture2D texture = RelicTexture.Value;

            // Choose frame based on item.placeStyle (based on X frame)
            int frameY = tile.TileFrameX / FrameWidth;
            Rectangle frame = texture.Frame(HorizontalFrames, VerticalFrames, 0, frameY);

            Vector2 origin = frame.Size() / 2f;
            Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);
            Color color = Lighting.GetColor(p.X, p.Y);

            // Use alternate direction if placed facing right
            bool flipped = tile.TileFrameY / FrameHeight != 0;
            SpriteEffects effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Make it float up/down with a sine wave
            const float TwoPi = (float)Math.PI * 2f;
            float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
            Vector2 drawPos = worldPos - Main.screenPosition + new Vector2(0f, -40f + offset * 4f);

            // Draw the relic texture
            spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

            // Draw a pulsating glow effect around the relic
            float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
            Color glow = color * 0.1f * scale;
            glow.A = 0;

            for (float t = 0f; t < 1f; t += 355f / (678f * (float)Math.PI))
            {
                Vector2 glowOffset = (TwoPi * t).ToRotationVector2() * (6f + offset * 2f);
                spriteBatch.Draw(texture, drawPos + glowOffset, frame, glow, 0f, origin, 1f, effects, 0f);
            }
        }
    }
}
