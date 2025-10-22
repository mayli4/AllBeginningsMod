using AllBeginningsMod.Core;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace AllBeginningsMod.Common.Rendering;

public interface ICustomLayerTile {
    public void DrawSpecialLayer(int i, int j, TileDrawLayer layer, SpriteBatch spriteBatch) { }
}

public enum TileDrawLayer {
    Background = 1,
    BehindTiles = 2,
    AboveTiles = 4,
    Foreground = 8,
    PostDrawTiles = 22
}

public sealed class CustomTileRendering : ILoadable {
    private static readonly Dictionary<TileDrawLayer, List<Point16>> _solidSpecialPoints = new Dictionary<TileDrawLayer, List<Point16>>();
    private static readonly Dictionary<TileDrawLayer, List<Point16>> _nonSolidSpecialPoints = new Dictionary<TileDrawLayer, List<Point16>>();

    public void Load(Mod mod) {
        ResetSpecialPointsCache(_solidSpecialPoints);
        ResetSpecialPointsCache(_nonSolidSpecialPoints);

        CommonHooks.DrawThingsBehindNonSolidSolidTilesEvent += DrawBehindNonSolidTiles;
        CommonHooks.DrawDustsEvent += DrawThingsAbovePlayersEvent;
        CommonHooks.ClearTileDrawingCachesEvent += ClearTiles;
    }

    private void DrawThingsAbovePlayersEvent() {
        if(LayerEmpty(TileDrawLayer.Foreground))
            return;

        Main.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        DrawCachedPoints(TileDrawLayer.Foreground);
        Main.spriteBatch.End();
    }

    public void Unload() { }

    private void DrawBehindNonSolidTiles() {
        if(LayerEmpty(TileDrawLayer.Background))
            return;
        DrawCachedPoints(TileDrawLayer.Background);
    }

    private void DrawBehindSolidTiles() {
        if(LayerEmpty(TileDrawLayer.BehindTiles))
            return;
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        DrawCachedPoints(TileDrawLayer.BehindTiles);
        Main.spriteBatch.End();
    }

    private void DrawAboveSolidTiles() {
        if(LayerEmpty(TileDrawLayer.AboveTiles))
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        DrawCachedPoints(TileDrawLayer.AboveTiles);
        Main.spriteBatch.End();
    }

    private void DrawPostDrawTiles() {
        if(LayerEmpty(TileDrawLayer.PostDrawTiles))
            return;
        DrawCachedPoints(TileDrawLayer.PostDrawTiles);
    }

    public static void AddSpecialDrawingPoint(int x, int y, TileDrawLayer layer, bool nonSolid = false) {
        if(nonSolid)
            _nonSolidSpecialPoints[layer].Add(new Point16(x, y));
        else
            _solidSpecialPoints[layer].Add(new Point16(x, y));
    }

    public static void ClearTiles(bool solidLayer) {
        if(solidLayer)
            ResetSpecialPointsCache(_solidSpecialPoints);
        else
            ResetSpecialPointsCache(_nonSolidSpecialPoints);
    }

    public static bool LayerEmpty(TileDrawLayer layer) => _nonSolidSpecialPoints[layer].Count + _solidSpecialPoints[layer].Count == 0;

    public static void DrawCachedPoints(TileDrawLayer layer) {
        DrawCachedPoints(layer, _nonSolidSpecialPoints);
        DrawCachedPoints(layer, _solidSpecialPoints);
    }

    public static void DrawCachedPoints(TileDrawLayer layer, Dictionary<TileDrawLayer, List<Point16>> dict) {
        for(int i = 0; i < dict[layer].Count; i++) {
            Point16 tilePos = dict[layer][i];
            ushort type = Main.tile[tilePos].TileType;
            if(TileLoader.GetTile(type) is ICustomLayerTile tile)
                tile.DrawSpecialLayer(dict[layer][i].X, dict[layer][i].Y, layer, Main.spriteBatch);
        }
    }

    public static void ResetSpecialPointsCache(Dictionary<TileDrawLayer, List<Point16>> dict) {
        dict.Clear();
        dict.Add(TileDrawLayer.Background, new List<Point16>());
        dict.Add(TileDrawLayer.BehindTiles, new List<Point16>());
        dict.Add(TileDrawLayer.AboveTiles, new List<Point16>());
        dict.Add(TileDrawLayer.Foreground, new List<Point16>());
        dict.Add(TileDrawLayer.PostDrawTiles, new List<Point16>());
    }
}