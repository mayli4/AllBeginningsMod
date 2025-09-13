using AllBeginningsMod.Utilities;

namespace AllBeginningsMod.Content.Tiles.AbandonedShack;

public class ContainerGhostTile : ModTile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        
        AddMapEntry(Color.Gray);
    }
}