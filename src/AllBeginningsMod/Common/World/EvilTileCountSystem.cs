using AllBeginningsMod.Content.Tiles.Corruption;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common.World;

public class EvilTileCountSystem : ModSystem {
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        Main.SceneMetrics.EvilTileCount += tileCounts[ModContent.TileType<OvergrownCorruptAsh>()] 
                                           + tileCounts[ModContent.TileType<CorruptAsh>()]
                                           + tileCounts[ModContent.TileType<OvergrownCorruptAshFoliage>()];
    }
}