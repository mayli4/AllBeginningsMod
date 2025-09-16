using AllBeginningsMod.Common.World;
using AllBeginningsMod.Utilities;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Tiles.AbandonedShack;

internal sealed class ContainerWorkbench : ModTile {
    public override string Texture => Textures.Tiles.AbandonedShack.KEY_ContainerWorkbenchTile;
    
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = false;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;
        
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.Width = 11;
        TileObjectData.newTile.Height = 1;
        
        TileObjectData.newTile.Origin = new Point16(5, 0);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        
        TileObjectData.newTile.CoordinateHeights = new int[] { 22 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.DrawYOffset = -4;
        
        TileObjectData.addTile(Type);
        
        AddMapEntry(Color.Brown);

        DustType = DustID.WoodFurniture;
        AdjTiles = [TileID.WorkBenches];
        HitSound = SoundID.Dig;
    }
    
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        if(!TileObjectData.IsTopLeft(i, j))
            return;
        
        Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
		var worldPos = new Vector2(i, j - 3) * 16;
		var drawPos = worldPos - Main.screenPosition + Helper.TileOffset;

        var tex = Textures.Tiles.AbandonedShack.ContainerWorkbenchTile_Extras.Value;

        if(PointsOfInterestSystem.LocalPlayerInShack) {
            Lighting.AddLight(worldPos + new Vector2(152, 20), Color.DarkOrange.ToVector3());
            if(Main.rand.NextBool(10)) {
                Dust.NewDust(worldPos + new Vector2(152, 20), 3, 3, DustID.Torch);   
            }
        }
        
        spriteBatch.Draw(tex, drawPos, Lighting.GetColor(i, j));
    }
}