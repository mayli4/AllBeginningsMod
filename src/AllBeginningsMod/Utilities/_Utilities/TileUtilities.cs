using Terraria;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Utilities;

public static class TileUtilities {
    public static void GetTopLeft(ref int i, ref int j) {
        var tile = Framing.GetTileSafely(i, j);
        var data = TileObjectData.GetTileData(tile);

        if (data is null)
            return;

        (i, j) = (i - tile.TileFrameX % data.CoordinateFullWidth / 18, j - tile.TileFrameY % data.CoordinateFullHeight / 18);
    }
    
    /// <summary> Tries to place or extend a vine at the given coordinates. </summary>
    /// <param name="i"> The tile's X coordinate. </param>
    /// <param name="j"> The tile's Y coordinate. </param>
    /// <param name="type"> The tile's type. </param>
    /// <param name="maxLength"> The maximum length this vine can grow. Does NOT instantly grow a vine of the given length. </param>
    /// <param name="reversed"> Whether this vine grows from the ground up. </param>
    /// <param name="sync"> Whether the tile changes should be automatically synced. </param>
    /// <returns> Whether the tile was successfully placed. </returns>
    public static bool GrowVine(int i, int j, int type, int maxLength = 15, bool reversed = false, bool sync = true) {
        if (reversed) {
            while (Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType == type)
                j++; //Move to the bottom of the vine

            for (int y = 0; y < maxLength; y++) {
                if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == type)
                    j--; //Move to the next available tile above
            }
        }
        else {
            while (Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].TileType == type)
                j--; //Move to the top of the vine

            for (int y = 0; y < maxLength; y++) {
                if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == type)
                    j++; //Move to the next available tile below
            }
        }

        if (Main.tile[i, j].TileType == type)
            return false; //The tile already exists; we've hit the max length

        WorldGen.PlaceObject(i, j, type, true);

        if (Main.tile[i, j].TileType != type)
            return false; //Tile placement failed

        if (Main.netMode != NetmodeID.SinglePlayer && sync)
            NetMessage.SendTileSquare(-1, i, j, 1, 1);

        return true;
    }
}