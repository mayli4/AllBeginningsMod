using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Tiles.Crimson;

public class CrimsonAsh : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Crimson.KEY_CrimsonAshTile;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        DustType = DustID.Crimson;
        
        AddMapEntry(new Color(107, 66, 63));
        Main.tileMerge[Type][ModContent.TileType<CrimsonAshGrass>()] = true;
        
        TileLoader.RegisterConversion(TileID.Ash, BiomeConversionID.Crimson, ConvertToCrimson);
    }
    
    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }
        
    public bool ConvertToCrimson(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch (conversionType) {
            case BiomeConversionID.Chlorophyte:
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.Ash);
                return;
            case BiomeConversionID.Sand:
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonAsh>());
                return;

        }
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Crimson);
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
        WorldGen.TileMergeAttempt(-2, TileID.Ash, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }
}

public class CrimsonAshItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Crimson.KEY_CrimsonAshItem;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CrimsonAsh>());
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