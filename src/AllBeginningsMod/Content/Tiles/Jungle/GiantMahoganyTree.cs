using Terraria.ID;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AllBeginningsMod.Content.Tiles.Jungle;

internal sealed class GiantMahoganyTree : ModTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_GiantJungleTreeTile;
    
    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;
        
        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

        DustType = DustID.RichMahogany;
        
        AddMapEntry(new Color(75, 86, 119));
    }

    private bool IsTreeTile(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == Type;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tile = Main.tile[i, j];
        int originalFrameX = tile.TileFrameX;
        int originalFrameY = tile.TileFrameY;
        
        int bottomTileType = -1;
        if (Main.tile[i, j + 1].HasTile)
            bottomTileType = Main.tile[i, j + 1].TileType;

        int topTileType = -1;
        if (Main.tile[i, j - 1].HasTile)
            topTileType = Main.tile[i, j - 1].TileType;

        tile.TileFrameY = 50;
        
        return true;
    }

    public static bool ValidTileToGrowOn(Tile t) => t.HasUnactuatedTile && !t.IsHalfBlock && t.Slope == SlopeType.Solid && 
                                                    (t.TileType == TileID.Stone);
    
    public override void RandomUpdate(int i, int j) {
        WorldGen.TileFrame(i - 1, j);
    }
}

internal class FrameTest : ModSystem {
    public override void PostUpdateEverything() {
        Vector2 mouseWorldPosition = Main.MouseWorld;
        int tileX = (int)(mouseWorldPosition.X / 16f);
        int tileY = (int)(mouseWorldPosition.Y / 16f);
        Point spawnPoint = new Point(tileX, tileY);

        if(Main.keyState.IsKeyDown(Keys.H)) {
            WorldGen.TileFrame(tileX, tileY, true);
        }
    }
}