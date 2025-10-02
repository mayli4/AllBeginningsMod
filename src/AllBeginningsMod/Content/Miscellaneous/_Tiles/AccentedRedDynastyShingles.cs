using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Miscellaneous;

public class AccentedRedDynastyShinglesTile : ModTile {
    public override string Texture => Textures.Tiles.Decoration.KEY_AccentedRedDynastyShinglesTile;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        DustType = DustID.DynastyShingle_Red;
        
        AddMapEntry(new Color(149, 69, 68));
    }
}

public class AccentedRedDynastyShinglesItem : ModItem {
    public override string Texture => Textures.Tiles.Decoration.KEY_AccentedRedDynastyShinglesItem;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<AccentedRedDynastyShinglesTile>());
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