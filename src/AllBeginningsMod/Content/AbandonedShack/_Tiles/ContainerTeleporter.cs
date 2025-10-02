using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.AbandonedShack;

internal sealed class ContainerTeleporter : ModTile {
    public override string Texture => Textures.Tiles.AbandonedShack.KEY_ContainerTeleporter;
    
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;
        
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 1;
        
        TileObjectData.newTile.Origin = new Point16(1, 0);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
        
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Gray);

        DustType = DustID.Teleporter;
        HitSound = SoundID.Dig;
    }
}