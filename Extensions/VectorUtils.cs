using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace EBF.Extensions
{
    public static class VectorUtils
    {
        /// <summary>
        /// Converts polar vectors into cartesian vectors.
        /// </summary>
        /// <param name="radius">The length of the vector.</param>
        /// <param name="theta">The angle of the vector.</param>
        /// <returns>A cartesian vector (A vector that has x and y coordinates).</returns>
        public static Vector2 Polar(float radius, float theta) => new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * radius;

        /// <summary>
        /// Shortcut to generate a randomized vector in any direction inside a square.
        /// </summary>
        /// <param name="size">Optionally multiply the valid area.</param>
        /// <returns>A vector where (X = -1 to 1, Y = -1 to 1).</returns>
        public static Vector2 Random(float size = 1) => Main.rand.NextVector2Square(-size, size);

        /// <summary>
        /// Scans an area for any ground.
        /// </summary>
        /// <param name="checkPosition">Top left starting position of the scan.</param>
        /// <param name="checkArea">How wide and tall the area to scan is.</param>
        /// <param name="groundPosition">The position of the found tile. Note that this will be the leftmost tile of the row.</param>
        /// <param name="checkPlatforms">Optionally ignore platforms. Set to false by default.</param>
        /// <returns>true if ground was found.</returns>
        public static bool TryGetGroundPosition(Vector2 checkPosition, Vector2 checkArea, out Vector2 groundPosition, bool checkPlatforms = false)
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

        public static Vector2 ToGroundPosition(this Vector2 checkPosition)
        {
            Point pos = checkPosition.ToTileCoordinates();
            for (; pos.Y < Main.maxTilesY - 10 && Main.tile[pos.X, pos.Y] != null && !WorldGen.SolidTile3(pos.X, pos.Y); pos.Y++) { }

            return new Vector2(pos.X * 16 + 8, pos.Y * 16);
        }

        /// <summary>
        /// Checks every tile above the given position until air is found.
        /// </summary>
        public static Vector2 ToSurfacePosition(this Vector2 checkPosition)
        {
            Point pos = checkPosition.ToTileCoordinates();
            for (; pos.Y > 10 && Main.tile[pos.X, pos.Y] != null && WorldGen.SolidTile2(pos.X, pos.Y); pos.Y--) { }

            return new Vector2(pos.X * 16 + 8, pos.Y * 16);
        }
    }
}
