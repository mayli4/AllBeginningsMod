using AllBeginningsMod.Content.Tiles.Corruption;
using AllBeginningsMod.Content.Tiles.Crimson;
using System;
using Terraria;
using Terraria.ModLoader;
using CorruptAsh = AllBeginningsMod.Content.Tiles.Corruption.CorruptAsh;

namespace AllBeginningsMod.Core.World;

public class EvilTileCountSystem : ModSystem {
    internal static int[] CorruptTypes;
    private int _corruptCount;

    public static bool InUnderworldCorruption => ModContent.GetInstance<EvilTileCountSystem>()._corruptCount >= 200;

    public override void SetStaticDefaults() => CorruptTypes = [ModContent.TileType<CorruptAsh>(), ModContent.TileType<OvergrownCorruptAsh>()];

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        _corruptCount = 0;

        foreach (int type in CorruptTypes)
            _corruptCount += tileCounts[type];
    }
}