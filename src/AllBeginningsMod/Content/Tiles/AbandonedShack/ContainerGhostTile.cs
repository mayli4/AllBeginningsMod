using AllBeginningsMod.Utilities;

namespace AllBeginningsMod.Content.Tiles.AbandonedShack;

internal sealed class ContainerGhostTile : ModTile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        
        AddMapEntry(Color.Red);
    }
}

internal sealed class ContainerGhostWall : ModWall {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override void SetStaticDefaults() {
        Main.tileBlockLight[Type] = true;
        
        AddMapEntry(Color.DarkRed);
    }
}