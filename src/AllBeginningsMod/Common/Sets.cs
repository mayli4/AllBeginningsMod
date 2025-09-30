using Terraria.ID;

namespace AllBeginningsMod.Common;

internal sealed class Sets : ModSystem {
    public static bool[] AlwaysPreventTileBreakIfOnTopOfIt { get; private set; } = [];

    public override void ResizeArrays() {
        base.ResizeArrays();
        
        AlwaysPreventTileBreakIfOnTopOfIt = new bool[TileLoader.TileCount];
        NPCID.Sets.Factory.CreateNamedSet(Mod, nameof(AlwaysPreventTileBreakIfOnTopOfIt)).RegisterCustomSet(true);
    }
}