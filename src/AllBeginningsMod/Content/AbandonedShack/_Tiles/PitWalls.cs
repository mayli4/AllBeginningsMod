using Terraria.ID;

namespace AllBeginningsMod.Content.AbandonedShack;

internal sealed class GrassyDirtWall : ModWall {
    public override string Texture => Assets.Textures.Tiles.AbandonedShack.GrassyDirtWall.KEY;

    public override void SetStaticDefaults() {
        Main.tileBlockLight[Type] = true;

        AddMapEntry(Color.Brown);
    }
}

internal sealed class GrassyDirtWallItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.AbandonedShack.GrassyDirtWall.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<GrassyDirtWall>());
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

internal sealed class SmoothStoneWall : ModWall {
    public override string Texture => Assets.Textures.Tiles.AbandonedShack.SmoothStoneWall.KEY;

    public override void SetStaticDefaults() {
        Main.tileBlockLight[Type] = true;

        AddMapEntry(Color.Gray);
    }
}

internal sealed class SmoothStoneWallItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.AbandonedShack.SmoothStoneWall.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<SmoothStoneWall>());
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