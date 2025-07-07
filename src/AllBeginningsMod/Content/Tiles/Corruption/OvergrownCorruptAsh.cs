using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public class OvergrownCorruptAsh : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_OvergrownCorruptAshTile;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Grass"]);

        DustType = DustID.CorruptPlants;
        
        AddMapEntry(new Color(69, 68, 114));
        
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<CorruptAsh>();
        TileID.Sets.CanBeDugByShovel[Type] = true;
        
        Main.tileMerge[Type][ModContent.TileType<CorruptAsh>()] = true;
    }
    
    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }
    
    public override void RandomUpdate(int i, int j) {
        if (SpreadUtilities.Spread(i, j, Type, 2, ModContent.TileType<CorruptAsh>()))
            NetMessage.SendTileSquare(-1, i, j, 3, TileChangeType.None); //Try spread grass

        GrowTiles(i, j);
    }

    protected virtual void GrowTiles(int i, int j) {
        var tile = Framing.GetTileSafely(i, j);
        var tileAbove = Framing.GetTileSafely(i, j - 1);

        //try place foliage
        if (WorldGen.genRand.NextBool(10) && !tileAbove.HasTile && tileAbove.LiquidAmount < 80) {
            if (!tile.BottomSlope && !tile.TopSlope && !tile.IsHalfBlock && !tile.TopSlope) {
                tileAbove.TileType = (ushort)ModContent.TileType<OvergrownCorruptAshFoliage>();
                tileAbove.HasTile = true;
                tileAbove.TileFrameY = 0;
                tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(8) * 18);
                WorldGen.SquareTileFrame(i, j + 1, true);
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }
    }
}