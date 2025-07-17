using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace AllBeginningsMod.Content.Tiles.Corruption;

//todo make it behave like an actuall herb

public enum PlantStage : byte {
    Planted,
    Growing,
    Grown
}

public class CorruptFireblossom : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_CorruptFireblossom;
    
    public override void SetStaticDefaults() {
        Main.tileCut[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;
        
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<OvergrownCorruptAsh>()];

        DustType = DustID.CursedTorch;
        HitSound = SoundID.Grass;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(230, 255, 63));
        
        TileLoader.RegisterConversion(TileID.BloomingHerbs, BiomeConversionID.Corruption, ConvertToCorruption);
    }
    
    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }
    
    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if(Main.rand.NextBool(10)) {
            yield return new Item(ItemID.Fireblossom, Main.rand.Next(1, 3));
        }
    }
}