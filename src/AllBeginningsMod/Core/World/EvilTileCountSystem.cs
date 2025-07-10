using AllBeginningsMod.Content.Tiles.Corruption;
using AllBeginningsMod.Content.Tiles.Crimson;
using System;
using Terraria;
using Terraria.ModLoader;
using CorruptAsh = AllBeginningsMod.Content.Tiles.Corruption.CorruptAsh;

namespace AllBeginningsMod.Core.World;

public class EvilTileCountSystem : ModSystem {
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        //corruption
        Main.SceneMetrics.EvilTileCount += tileCounts[ModContent.TileType<OvergrownCorruptAsh>()] 
                                           + tileCounts[ModContent.TileType<CorruptAsh>()]
                                           + tileCounts[ModContent.TileType<OvergrownCorruptAshFoliage>()];
        
        //crimson
        Main.SceneMetrics.EvilTileCount += tileCounts[ModContent.TileType<CrimsonAshGrass>()]
                                           + tileCounts[ModContent.TileType<CrimsonAsh>()];
    }
}