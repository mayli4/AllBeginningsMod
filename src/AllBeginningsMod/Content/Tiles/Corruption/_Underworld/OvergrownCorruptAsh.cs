using AllBeginningsMod.Core.World;
using AllBeginningsMod.Content.Tiles.Decoration;
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
        Main.tileMergeDirt[Type] = true;
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
        
        TileLoader.RegisterConversion(TileID.AshGrass, BiomeConversionID.Corruption, ConvertToCorruption);
    }
    
    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch (conversionType) {
            case BiomeConversionID.Chlorophyte:
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.AshGrass);
                return;
            case BiomeConversionID.Sand:
            case BiomeConversionID.Corruption:
                WorldGen.ConvertTile(i, j, ModContent.TileType<OvergrownCorruptAsh>());
                return;
            
        }
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Corruption);
            
        if (SpreadUtilities.Spread(i, j, Type, 2, ModContent.TileType<CorruptAsh>()))
            NetMessage.SendTileSquare(-1, i, j, 3); // try spread grass

        GrowTiles(i, j);
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
        WorldGen.TileMergeAttempt(-2, TileID.Ash, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }
    
    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
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
        
        if (Main.rand.NextBool(5) && WorldGen.GrowMoreVines(i, j) && Main.tile[i, j + 1].LiquidType != LiquidID.Lava)
            TileUtilities.GrowVine(i, j + 1, ModContent.TileType<UnderworldCorruptVines>());
    }
}