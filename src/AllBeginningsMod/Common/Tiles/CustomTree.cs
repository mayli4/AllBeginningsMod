using System.Collections.Generic;
using Terraria.DataStructures;

namespace AllBeginningsMod.Common.Tiles;

internal abstract class CustomTree : ModTile {
    public virtual int TreeHeight => WorldGen.genRand.Next(20, 40);

    internal readonly HashSet<Point16> ShakePoints = [];

    /// <summary>
    ///     Whether the given tile is a treetop.
    /// </summary>
    /// <returns>false by default.</returns>
    public virtual bool IsTreeTop() => false;

    public override void Load() {

    }

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;
    }

    public virtual void DrawFoliage(SpriteBatch spriteBatch, int i, int j) {

    }
}