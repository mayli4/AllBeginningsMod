using Terraria.ModLoader.IO;

namespace AllBeginningsMod.Common.World;

/// <summary>
///     Holds the locations and bounds of all generally important locations.
/// </summary>
internal sealed class PointsOfInterestSystem : ModSystem {
    public bool FoundOldbotShack = false;
    public static Point ShackPosition { get; internal set; } = Point.Zero;
    public static Rectangle ShackBounds { get; internal set; } = Rectangle.Empty;
    
    public override void SaveWorldData(TagCompound tag) {
        if (ShackPosition != Point.Zero) {
            tag["ShackPosition"] = ShackPosition.ToVector2();
        }
        
        if (ShackBounds != Rectangle.Empty) {
            tag["ShackBounds"] = ShackBounds;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        if (tag.TryGet("ShackPosition", out Vector2 position)) {
            ShackPosition = position.ToPoint();
        }
        
        if (tag.TryGet("ShackBounds", out Rectangle bounds)) {
            ShackBounds = bounds;
        }
    }
}