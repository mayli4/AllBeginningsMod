using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Utilities;
public static class Interpolation {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseLerp(float a, float b, float value) {
        return (value - a) / (b - a);
    }
}
