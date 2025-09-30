using AllBeginningsMod.Common;
using AllBeginningsMod.Common.World;
using AllBeginningsMod.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
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
        
        AddMapEntry(new Color(86, 38, 55), Language.GetText("MapObject.Tree"));
        
        KillTileHooks.OverrideKillTileEvent += PreventTileBreakIfOnTopOfIt;
    }

    private bool? PreventTileBreakIfOnTopOfIt(int i, int j, int type) {
        if (type == Type)
            return true;

        return null;
    }

    private bool IsTreeTile(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == Type;
    }
    
    public static bool ValidTileToGrowOn(Tile t) => t.HasUnactuatedTile && !t.IsHalfBlock && t.Slope == SlopeType.Solid && 
                                                    (t.TileType == TileID.JungleGrass);

    private bool IsTreeTop(int x, int y) => Main.tile[x, y].TileFrameY == 9000;

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
 
        tile.TileFrameY = 0;
        
        return true;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Tile t = Main.tile[i, j];
        if (TileDrawing.IsVisible(t) && t.TileFrameY == 10) //Treetop
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }
    
    public static float GetSway(int i, int j, double factor = 0) {
        if (factor == 0)
            factor = TileSwaying.Instance.TreeWindCounter;

        return Main.instance.TilesRenderer.GetWindCycle(i, j, factor) * .4f;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile t = Main.tile[i, j];
        Texture2D texture = TextureAssets.Tile[Type].Value;
        if (t.TileColor != PaintID.None) {
            Texture2D paintedTex = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(Type, 0, t.TileColor);
            texture = paintedTex ?? texture;
        }
        
        var trunkTexture = TextureAssets.Tile[Type].Value;
        
        var worldPos = new Vector2(i + 1, j) * 16;
        var drawPos = worldPos - Main.screenPosition;
        
        float rotation = GetSway(i + 1, j) * .04f;
        float leavesRotation = GetSway(i + 1, j) * .1f;

        var rect = new Rectangle(164, 36, 510, 226);
        var leavesRect = new Rectangle(676, 36, 510, 226);
        var underLeavesRect = new Rectangle(1188, 36, 510, 226);
        var origin = new Vector2(rect.Width / 2, rect.Height);
        
        spriteBatch.Draw(
            texture,
            drawPos + new Vector2(0, 0),
            underLeavesRect,
            Lighting.GetColor(i, j),
            leavesRotation,
            origin, 
            1f,
            SpriteEffects.None,
            0f
        );
        
        spriteBatch.Draw(
            trunkTexture,
            drawPos + new Vector2(0, 0),
            rect,
            Lighting.GetColor(i, j),
            rotation,
            origin, 
            1f,
            SpriteEffects.None,
            0f
        );
        
        spriteBatch.Draw(
            texture,
            drawPos + new Vector2(0, 0),
            leavesRect,
            Lighting.GetColor(i, j),
            leavesRotation,
            origin, 
            1f,
            SpriteEffects.None,
            0f
        );
    }

    public override void RandomUpdate(int i, int j) {
        
    }
}

internal class FrameTest : ModSystem {
    private bool IsTreeTile(int x, int y) {
        if (!WorldGen.InWorld(x, y)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<GiantMahoganyTree>();
    }
    
    public override void PostUpdateEverything() {
        Vector2 mouseWorldPosition = Main.MouseWorld;
        int tileX = (int)(mouseWorldPosition.X / 16f);
        int tileY = (int)(mouseWorldPosition.Y / 16f);

        if(Main.keyState.IsKeyDown(Keys.H) && IsTreeTile(tileX, tileY)) {
            WorldGen.TileFrame(tileX, tileY, true);
        }
    }
}