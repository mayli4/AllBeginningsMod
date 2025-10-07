using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Jungle;

internal sealed class CorpseFlower : ModTile {
    public override string Texture => Textures.Tiles.Jungle.KEY_CorpseFlowerBase;
    
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        
        TileObjectData.newTile.Origin = new Point16(0, 0);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        
        TileObjectData.newTile.CoordinateHeights = new [] { 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 2;
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Brown);

        DustType = DustID.RichMahogany;
        HitSound = SoundID.Dig;
    }
}