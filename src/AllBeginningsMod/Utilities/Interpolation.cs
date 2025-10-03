using System;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Utilities;

internal static class Interpolation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseLerp(float a, float b, float value) {
        return Math.Clamp((value - a) / (b - a), 0f, 1f);
    }
}
