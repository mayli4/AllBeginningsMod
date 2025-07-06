using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Tiles.Corruption;

public class CorruptAsh : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_CorruptAshTile;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        DustType = DustID.Corruption;
        
        AddMapEntry(new Color(69, 68, 114));
    }
    
    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
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