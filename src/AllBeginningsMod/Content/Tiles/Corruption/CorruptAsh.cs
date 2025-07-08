using AllBeginningsMod.Common.World;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public class CorruptAsh : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_CorruptAshTile;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        DustType = DustID.Corruption;
        
        AddMapEntry(new Color(69, 68, 114));
        Main.tileMerge[Type][ModContent.TileType<OvergrownCorruptAsh>()] = true;
        
        TileLoader.RegisterConversion(TileID.Ash, BiomeConversionID.Corruption, ConvertToCorruption);
    }
    
    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }
        
    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch (conversionType) {
            case BiomeConversionID.Chlorophyte:
            case BiomeConversionID.Purity:
            case BiomeConversionID.Sand:
            case BiomeConversionID.Corruption:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptAsh>());
                return;

        }
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Corruption);
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
        WorldGen.TileMergeAttempt(-2, TileID.Ash, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }
}

public class CorruptAshItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_CorruptAshItem;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CorruptAsh>());
        Item.width = 16;
        Item.height = 16;
        Item.value = 5;
        
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
    }
}