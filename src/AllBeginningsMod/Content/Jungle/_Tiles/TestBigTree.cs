using AllBeginningsMod.Common.World;
using Terraria.ID;
using Terraria.Localization;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AllBeginningsMod.Content.Jungle;

public class TestBigTree : ModTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_TestBigTree;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;
        MineResist = 1f;
        
        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

        DustType = DustID.RichMahogany;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        return base.TileFrame(i, j, ref resetFrame, ref noBreak);   
    }
    
    public static bool GrowTree(int x, int y, bool needsSapling = true, int generateOverrideHeight = 0) {
        var type = ModContent.TileType<TestBigTree>();

        void GenBaseOnlyOutermost(int i, int j) {
            WorldGen.PlaceTile(i - 1, j, type, mute: true, forced: true);
            WorldGen.PlaceTile(i + 3, j, type, mute: true, forced: true);
        }
        
        int minHeightSegments = 20;
        int maxHeightSegments = 40; 
        
        int treeHeightSegments = generateOverrideHeight > 0 ? generateOverrideHeight : WorldGen.genRand.Next(minHeightSegments, maxHeightSegments + 1);

        GenBaseOnlyOutermost(x, y);

        for (int h = 0; h < treeHeightSegments; h++) {
            int currentY = y - h;

            WorldGen.PlaceTile(x, currentY, type, mute: true, forced: true);
            WorldGen.PlaceTile(x + 1, currentY, type, mute: true, forced: true);
            WorldGen.PlaceTile(x + 2, currentY, type, mute: true, forced: true);
        }

        int frameStartX = x - 1; 
        int frameWidth = 5;      
        for (int h = 0; h < treeHeightSegments; h++) {
            for (int iOffset = 0; iOffset < frameWidth; iOffset++) {
                 WorldGen.SquareTileFrame(frameStartX + iOffset, y - h, true);
            }
            
            if (h == 0) { 
                frameStartX = x;
                frameWidth = 3;
            }
        }
        
        return true;
    }
}

internal class FrameTest : ModSystem {
    public override void PostUpdateEverything() {
        Vector2 mouseWorldPosition = Main.MouseWorld;
        int tileX = (int)(mouseWorldPosition.X / 16f);
        int tileY = (int)(mouseWorldPosition.Y / 16f);

        if(Main.keyState.IsKeyDown(Keys.H) && Main.keyState.IsKeyDown(Keys.LeftShift) && !Main.oldKeyState.IsKeyDown(Keys.H)) {
            //WorldGen.TileFrame(tileX, tileY, true);
            
            TestBigTree.GrowTree(tileX, tileY);
        }
    }
}