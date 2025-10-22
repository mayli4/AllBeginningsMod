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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SineInEasing(float amount, float degree = 1f)
        => 1f - (float)Math.Cos(amount * MathHelper.Pi / 2f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SineOutEasing(float amount, float degree = 1f)
        => (float)Math.Sin(amount * MathHelper.Pi / 2f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SineInOutEasing(float amount, float degree = 1f)
        => -((float)Math.Cos(amount * MathHelper.Pi) - 1) / 2f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SineBumpEasing(float amount, float degree = 1f)
        => (float)Math.Sin(amount * MathHelper.Pi);

    public static float LinearEasing(float amount, float degree = 1f) => amount;
    //Exponential
    public static float ExpInEasing(float amount, float degree = 1f) => amount == 0f ? 0f : (float)Math.Pow(2, 10f * amount - 10f);
    public static float ExpOutEasing(float amount, float degree = 1f) => amount == 1f ? 1f : 1f - (float)Math.Pow(2, -10f * amount);
    public static float ExpInOutEasing(float amount, float degree = 1f) => amount == 0f ? 0f : amount == 1f ? 1f : amount < 0.5f ? (float)Math.Pow(2, 20f * amount - 10f) / 2f : (2f - (float)Math.Pow(2, -20f * amount - 10f)) / 2f;
    //circular
    public static float CircInEasing(float amount, float degree = 1f) => (1f - (float)Math.Sqrt(1 - Math.Pow(amount, 2f)));
    public static float CircOutEasing(float amount, float degree = 1f) => (float)Math.Sqrt(1 - Math.Pow(amount - 1f, 2f));
    public static float CircInOutEasing(float amount, float degree = 1f) => amount < 0.5 ? (1f - (float)Math.Sqrt(1 - Math.Pow(2 * amount, 2f))) / 2f : ((float)Math.Sqrt(Math.Max(1 - Math.Pow(-2f * amount - 2f, 2f), 0)) + 1f) / 2f;


    public static float EaseInOutBack(float amount, float degree) {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return amount < 0.5
            ? (float)(Math.Pow(2 * amount, 2) * ((c2 + 1) * 2 * amount - c2)) / 2f
            : (float)(Math.Pow(2 * amount - 2, 2) * ((c2 + 1) * (amount * 2 - 2) + c2) + 2) / 2f;
    }

    public static float EaseQuarticIn(float x) {
        return (float)Math.Pow(x, 4);
    }

    public static float EaseQuarticOut(float x) {
        return 1f - EaseQuarticIn(1f - x);
    }

    public static float EaseQuarticInOut(float x) {
        return (x < 0.5f) ?
            8f * (float)Math.Pow(x, 4) :
            -8f * (float)Math.Pow(x, 4) + 32f * (float)Math.Pow(x, 3) - 48f * (float)Math.Pow(x, 2) + 32f * x - 7f;
    }

    public static float EaseQuinticIn(float x) {
        return (float)Math.Pow(x, 5);
    }

    public static float EaseQuinticOut(float x) {
        return 1f - EaseQuinticIn(1f - x);
    }

    public static float EaseQuinticInOut(float x) {
        return (x < 0.5f) ?
            16f * (float)Math.Pow(x, 5) :
            16f * (float)Math.Pow(x, 5) - 80f * (float)Math.Pow(x, 4) + 160f * (float)Math.Pow(x, 3) - 160f * (float)Math.Pow(x, 2) + 80f * x - 15f;
    }
}