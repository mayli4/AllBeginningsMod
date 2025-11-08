using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Miscellaneous;

public class BigPotTile : ModTile {
    public override string Texture => Assets.Textures.Tiles.Decoration.BigPotTile.KEY;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        DustType = DustID.Stone;

        AddMapEntry(new Color(73, 89, 97));
    }
}

public class BigPotItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Decoration.BigPotItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<BigPotTile>());
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