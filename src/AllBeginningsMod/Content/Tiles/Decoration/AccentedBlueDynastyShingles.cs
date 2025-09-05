using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Tiles.Decoration;

public class AccentedBlueDynastyShinglesTile : ModTile {
    public override string Texture => Textures.Tiles.Decoration.KEY_AccentedBlueDynastyShinglesTile;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        DustType = DustID.DynastyShingle_Blue;
        
        AddMapEntry(new Color(75, 86, 119));
    }
}

public class AccentedBlueDynastyShinglesItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Decoration.KEY_AccentedBlueDynastyShinglesItem;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<AccentedBlueDynastyShinglesTile>());
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