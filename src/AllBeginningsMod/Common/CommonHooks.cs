using System;
using Terraria.GameContent.Drawing;

namespace AllBeginningsMod.Core;

internal sealed class CommonHooks : ModSystem {
    public delegate void PlayerDrawingAction(bool afterProjectiles);
    
    public override void Load() {
        On_Main.DoDraw_WallsAndBlacks += DrawHook_BehindWalls;
        On_Main.DoDraw_Tiles_Solid += DrawHook_BehindTiles;
        On_Main.DoDraw_Tiles_NonSolid += DrawHook_BehindNonSolidTiles;
        On_Main.DrawPlayers_AfterProjectiles += DrawHook_AfterPlayers;
        On_Main.DrawGore += On_MainOnDrawGore;
        
        On_TileDrawing.PreDrawTiles += ClearForegroundStuff;
    }

    public static event Action DrawThingsOverGore;
    private void On_MainOnDrawGore(On_Main.orig_DrawGore orig, Main self) {
        orig(self);
        DrawThingsOverGore?.Invoke();
    }

    public override void Unload() {
        On_Main.DoDraw_WallsAndBlacks -= DrawHook_BehindWalls;
        On_Main.DoDraw_Tiles_Solid -= DrawHook_BehindTiles;
        On_Main.DoDraw_Tiles_NonSolid -= DrawHook_BehindNonSolidTiles;
        On_Main.DrawPlayers_AfterProjectiles -= DrawHook_AfterPlayers;
        
        On_TileDrawing.PreDrawTiles -= ClearForegroundStuff;
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
    
    public delegate void ClearTileCacheDelegate(bool solidLayer);
    public static event ClearTileCacheDelegate ClearTileDrawingCachesEvent;
    private static void ClearForegroundStuff(On_TileDrawing.orig_PreDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets) {
        orig(self, solidLayer, forRenderTargets, intoRenderTargets);

        //If we draw every frame or we draw into a RT, it means we're resetting stuff
        if (intoRenderTargets || Lighting.UpdateEveryFrame)
            ClearTileDrawingCachesEvent?.Invoke(solidLayer);
    }
}