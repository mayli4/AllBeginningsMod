using AllBeginningsMod.Content.Tiles.Corruption;
using AllBeginningsMod.Content.Tiles.Crimson;
using System;
using Terraria.ModLoader;
using CorruptAsh = AllBeginningsMod.Content.Tiles.Corruption.CorruptAsh;

namespace AllBeginningsMod.Core.World;

public class EvilTileCountSystem : ModSystem {
    internal static int[] CorruptTypes;
    internal static int[] CrimsonTypes;
    private int _corruptCount;
    private int _crimsonCount;

    public static bool InUnderworldCorruption => ModContent.GetInstance<EvilTileCountSystem>()._corruptCount >= 200;
    public static bool InUnderworldCrimson => ModContent.GetInstance<EvilTileCountSystem>()._crimsonCount >= 200;

    public override void SetStaticDefaults() {
        CorruptTypes = [ModContent.TileType<CorruptAsh>(), ModContent.TileType<OvergrownCorruptAsh>()];
        CrimsonTypes = [ModContent.TileType<CrimsonAsh>(), ModContent.TileType<CrimsonAshGrass>()];
    }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        _corruptCount = 0;
        _crimsonCount = 0;

        foreach (int type in CorruptTypes)
            _corruptCount += tileCounts[type];
        
        foreach (int type in CrimsonTypes)
            _crimsonCount += tileCounts[type];
    }
}