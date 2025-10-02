namespace AllBeginningsMod.Common.Tiles;

internal abstract class CustomTree : ModTile {
    public override void Load() {

    }

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;
    }

    
    /// <summary>
    ///     Whether the given tile is a treetop.
    /// </summary>
    /// <returns>false by default.</returns>
    public virtual bool IsTreeTop() => false;

    /// <summary>
    ///     Draws foliage over players.
    /// </summary>
    public virtual void DrawFoliageOver() { }
}