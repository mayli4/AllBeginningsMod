using System;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Utilities;

internal sealed class Easings {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public static float PolyInOutEasing(float amount, float degree = 2f) 
        => amount < 0.5f ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree) : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public static float PolyInEasing(float amount, float degree = 2f) 
        => (float)Math.Pow(amount, degree);
}