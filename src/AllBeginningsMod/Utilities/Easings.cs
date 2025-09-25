using System;

namespace AllBeginningsMod.Utilities;

public class Easings {
    public static float PolyInOutEasing(float amount, float degree = 2f) => amount < 0.5f ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree) : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;
    public static float PolyInEasing(float amount, float degree = 2f) => (float)Math.Pow(amount, degree);
    
    public static float BezierEase(float time) => time * time / (2f * (time * time - time) + 1f);
}