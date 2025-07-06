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
    }
    
    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }
}