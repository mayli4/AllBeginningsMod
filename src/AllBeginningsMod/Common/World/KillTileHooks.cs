namespace AllBeginningsMod.Common.World;

internal sealed class KillTileHooks : GlobalTile {
    public override void Load() {
        On_WorldGen.CanKillTile_int_int_refBoolean += PreventOrForceTileBreak;
    }

    public delegate bool? OverrideKillTileDelegate(int i, int j, int type);
    /// <summary>
    /// This is like CanKillTile, except it entirely overrides it, and gets ran in <see cref="WorldGen.CheckTileBreakability(int, int)"/> which prevents grass from being turned into dirt<br/>
    /// Used by modded trees to prevent grass break below, and to prevent vanilla tile break logic from running for them. (Vanilla tilebreak checks specific treebranch frames, which may not match our modded tilesheet format)
    /// </summary>
    public static event OverrideKillTileDelegate OverrideKillTileEvent;
    private bool PreventOrForceTileBreak(On_WorldGen.orig_CanKillTile_int_int_refBoolean orig, int i, int j, out bool blockDamaged) {
        blockDamaged = false;
        if(i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
            return false;
        if(!Main.tile[i, j].HasTile)
            return false;

        int type = Main.tile[i, j].TileType;

        if(j > 0) {
            Tile tileAbove = Main.tile[i, j - 1];
            if(tileAbove.HasTile && Sets.AlwaysPreventTileBreakIfOnTopOfIt[tileAbove.TileType] && type != tileAbove.TileType)
                return false;
        }

        if(OverrideKillTileEvent == null)
            return true;

        foreach(OverrideKillTileDelegate check in OverrideKillTileEvent.GetInvocationList()) {
            bool? result = check(i, j, type);
            if(result.HasValue)
                return result.Value;
        }

        return orig(i, j, out blockDamaged);
    }
}