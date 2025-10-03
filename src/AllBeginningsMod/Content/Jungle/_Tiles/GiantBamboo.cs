using AllBeginningsMod.Utilities;
using Terraria.ID;

namespace AllBeginningsMod.Content.Jungle;

internal sealed class GiantBamboo : ModTile {
    public override string Texture => Helper.PlaceholderTextureKey;
    
    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;
        MineResist = 2f;
        
        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

        DustType = DustID.RichMahogany;
        
        AddMapEntry(new Color(165, 168, 26));
    }
    
    public static bool ValidTileToGrowOn(Tile t) => t.TileType == TileID.JungleGrass || t.TileType == TileID.Mud;
}