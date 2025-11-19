using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EBF.Tiles.Furniture.BossRelics
{
    /// <summary>
    /// Common code for a Master Mode boss relic
    /// </summary>
    public abstract class EBFRelicBase : ModTile
    {
        private const int FrameWidth = 18 * 3;
        private const int FrameHeight = 18 * 4;
        private Asset<Texture2D> RelicHeadTexture;
        public override sealed string Texture => "EBF/Tiles/Furniture/BossRelics/RelicPedestal"; // Relic pedestal
        public abstract string RelicHeadTexturePath { get; } // Relic head (Must be overriden)
        public override sealed void Load() => RelicHeadTexture = ModContent.Request<Texture2D>(RelicHeadTexturePath); // Cache the head texture
        public override sealed void SetStaticDefaults()
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
        public override sealed bool CreateDust(int i, int j, ref int type) => false;
        public override sealed void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            // Forces pedestal to draw even if placeStyle differs
            tileFrameX %= FrameWidth;
            tileFrameY %= FrameHeight * 2; // Two placement directions: left/right
        }
        public override sealed void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            // Relic doesn't include the hovering part on its sheet, so we manually register a special draw point
            if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
            {
                Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
            }
        }
        public override sealed void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Ensure tile exists before drawing
            Point p = new(i, j);
            Tile tile = Main.tile[p.X, p.Y];
            if (!tile.HasTile)
                return;

            Texture2D texture = RelicHeadTexture.Value;
            Rectangle frame = texture.Bounds;

            Vector2 origin = frame.Size() / 2f;
            Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);
            Color color = Lighting.GetColor(p.X, p.Y);

            // Use alternate direction if placed facing right
            bool flipped = tile.TileFrameY / FrameHeight != 0;
            SpriteEffects effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Make it float up/down with a sine wave
            float offset = MathF.Sin(Main.GlobalTimeWrappedHourly * MathF.Tau / 5f);
            Vector2 drawPos = worldPos - Main.screenPosition + new Vector2(0f, -40f + offset * 4f);

            // Draw the relic texture
            spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

            // Draw a pulsating glow effect around the relic
            float scale = MathF.Sin(Main.GlobalTimeWrappedHourly * MathF.Tau / 2f) * 0.3f + 0.7f;
            Color glow = color * 0.1f * scale;
            glow.A = 0;

            for (float t = 0f; t < 1f; t += 355f / (678f * MathF.PI))
            {
                Vector2 glowOffset = (MathF.Tau * t).ToRotationVector2() * (6f + offset * 2f);
                spriteBatch.Draw(texture, drawPos + glowOffset, frame, glow, 0f, origin, 1f, effects, 0f);
            }
        }
    }
}
