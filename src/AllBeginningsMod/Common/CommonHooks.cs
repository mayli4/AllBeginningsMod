using System;

namespace AllBeginningsMod.Core;

internal sealed class CommonHooks : ModSystem {
    public delegate void PlayerDrawingAction(bool afterProjectiles);
    
    public override void Load() {
        On_Main.DoDraw_WallsAndBlacks += DrawHook_BehindWalls;
        On_Main.DoDraw_Tiles_Solid += DrawHook_BehindTiles;
        On_Main.DoDraw_Tiles_NonSolid += DrawHook_BehindNonSolidTiles;
        On_Main.DrawPlayers_AfterProjectiles += DrawHook_AfterPlayers;
    }
    
    public override void Unload() {
        On_Main.DoDraw_WallsAndBlacks -= DrawHook_BehindWalls;
        On_Main.DoDraw_Tiles_Solid -= DrawHook_BehindTiles;
        On_Main.DoDraw_Tiles_NonSolid -= DrawHook_BehindNonSolidTiles;
        On_Main.DrawPlayers_AfterProjectiles -= DrawHook_AfterPlayers;
    }

    public static event Action DrawThingsBehindWallsEvent;
    public static event Action DrawThingsOverWallsEvent;
    private void DrawHook_BehindWalls(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self) {
        DrawThingsBehindWallsEvent?.Invoke();
        orig(self);
        DrawThingsOverWallsEvent?.Invoke();
    }
    
    public static event Action DrawThingsBehindSolidTilesEvent;
    private void DrawHook_BehindTiles(On_Main.orig_DoDraw_Tiles_Solid orig, Main self) {
        DrawThingsBehindSolidTilesEvent?.Invoke();
        orig(self);
    }
    
    public static event Action DrawThingsBehindNonSolidSolidTilesEvent;
    private void DrawHook_BehindNonSolidTiles(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self) {
        DrawThingsBehindNonSolidSolidTilesEvent?.Invoke();
        orig(self);
    }
    
    public static event PlayerDrawingAction PreDrawPlayersEvent;
    public static event PlayerDrawingAction DrawThingsAbovePlayersEvent;

    private void DrawHook_AfterPlayers(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        PreDrawPlayersEvent?.Invoke(true);
        orig(self);
        DrawThingsAbovePlayersEvent?.Invoke(true);
    }
}