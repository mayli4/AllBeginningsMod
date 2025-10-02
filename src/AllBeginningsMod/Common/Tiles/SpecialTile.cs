using System.Collections.Generic;
using Terraria.GameContent.Drawing;

namespace AllBeginningsMod.Common.Tiles;

public interface ITileSpecialDrawn {
    void SpecialDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing);
}

internal record struct SpecialTile(ModTile Tile, Point Position);

internal sealed class TileSpecialDrawManager : ModSystem {
    internal static List<SpecialTile> Tiles = [];

    public static void AddSpecialPoint(ModTile modTile, int i, int j) {
        if(Tiles.Contains(new SpecialTile(modTile, new Point(i, j))) || modTile is not ITileSpecialDrawn) 
            return;
        
        Tiles.Add(new SpecialTile(modTile, new Point(i, j)));
    }

    public override void Load() {
        On_TileDrawing.PreDrawTiles += (orig, self, solidLayer, forRenderTargets, intoRenderTargets) => {
            orig.Invoke(self,solidLayer,forRenderTargets, intoRenderTargets);
            bool flag = intoRenderTargets || Lighting.UpdateEveryFrame;
            if (!solidLayer && flag) {
                Tiles.Clear();
            }
        };

        On_TileDrawing.DrawReverseVines += (orig, self) => {
            orig.Invoke(self);

            Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
            foreach ((ModTile modTile, Point position) in Tiles) {
                if (modTile is ITileSpecialDrawn tileFluent && modTile is not null) {
                    tileFluent.SpecialDraw(unscaledPosition, position, Main.spriteBatch, self);
                }
            }
        };
    }
}