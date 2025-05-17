﻿using Microsoft.Xna.Framework;
using Terraria;

namespace EBF.Extensions
{
    public static class VectorUtils
    {
        /// <summary>
        /// Scans an area for any ground.
        /// </summary>
        /// <param name="checkPosition">Top left starting position of the scan.</param>
        /// <param name="checkArea">How wide and tall the area to scan is.</param>
        /// <param name="groundPosition">The position of the found tile. Note that this will be the leftmost tile of the row.</param>
        /// <param name="checkPlatforms">Optionally ignore platforms. Set to false by default.</param>
        /// <returns>true if ground was found.</returns>
        public static bool GetGroundPosition(Vector2 checkPosition, Vector2 checkArea, out Vector2 groundPosition, bool checkPlatforms = false)
        {
            Point p = checkPosition.ToTileCoordinates();
            Point a = p + checkArea.ToTileCoordinates();

            for (int y = p.Y; y < a.Y; y++)
                for (int x = p.X; x < a.X; x++)
                {
                    Tile t = Framing.GetTileSafely(x, y);
                    if ((t.HasTile && Main.tileSolid[t.TileType] && !t.IsActuated) ||
                        (checkPlatforms && Main.tileSolidTop[t.TileType] && !t.IsActuated))
                    {
                        groundPosition = new Vector2(x * 16, y * 16);
                        return true;
                    }
                }

            groundPosition = Vector2.Zero;
            return false;
        }

        public static Vector2 GetGroundPosition(Vector2 checkPosition)
        {
            Point pos = checkPosition.ToTileCoordinates();
            for (; pos.Y < Main.maxTilesY - 10 && Main.tile[pos.X, pos.Y] != null && !WorldGen.SolidTile2(pos.X, pos.Y); pos.Y++) { }

            return new Vector2(pos.X * 16 + 8, pos.Y * 16);
        }
    }
}
