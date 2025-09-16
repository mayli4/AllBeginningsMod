namespace AllBeginningsMod.Utilities;

internal static class ColorExtensions {
    public static Color Additive(this Color color, byte newAlpha = 0) {
        var temp = color;
        temp.A = (byte)(temp.A * newAlpha / byte.MaxValue);
        return temp;
    }
}