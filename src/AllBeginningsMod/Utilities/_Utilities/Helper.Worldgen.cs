using System;
using Terraria.DataStructures;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Utilities;

public partial class Helper {
    /// <summary>
    /// Checks that all tiles above the given point are air
    /// </summary>
    /// <param name="start"></param>
    /// <param name="MaxScan"></param>
    /// <returns></returns>
    public static bool AirScanUp(Point16 start, int MaxScan) {
        if (start.Y - MaxScan < 0)
            return false;

        for (int k = 1; k <= MaxScan; k++) {
            if (Main.tile[start.X, start.Y - k].HasTile)
                return false;
        }

        return true;
    }
}