using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.AbandonedShack;

internal sealed class ContainerSmeltery : ModTile {
    public override string Texture => Textures.Tiles.AbandonedShack.KEY_ContainerSmeltery;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [18, 18];
        TileObjectData.addTile(Type);

        AddMapEntry(Color.Gray);

        DustType = DustID.Stone;
        AdjTiles = [TileID.Furnaces];
        HitSound = SoundID.Tink;
    }
}