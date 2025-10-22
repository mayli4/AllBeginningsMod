using Terraria.ID;
using Terraria.WorldBuilding;

namespace AllBeginningsMod.Common.World;

internal sealed class GrassAction : GenAction {
    public override bool Apply(Point origin, int x, int y, params object[] args) {
        if(!WorldGen.SolidTile(x, y - 1)) {
            Framing.GetTileSafely(x, y).TileType = TileID.Grass;
        }
        return UnitApply(origin, x, y, args);
    }
}