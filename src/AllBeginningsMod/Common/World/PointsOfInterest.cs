using AllBeginningsMod.Utilities;
using Terraria.ModLoader.IO;

namespace AllBeginningsMod.Common.World;

/// <summary>
///     Holds the locations and bounds of all generally important locations.
/// </summary>
internal sealed class PointsOfInterestSystem : ModSystem {
    public static bool FoundOldbotShack = false;
    public static Point ShackPosition { get; internal set; } = Point.Zero;
    public static Rectangle ShackBounds { get; internal set; } = Rectangle.Empty;
    public static bool LocalPlayerInShack;
    
    public override void SaveWorldData(TagCompound tag) {
        if (ShackPosition != Point.Zero) {
            tag["ShackPosition"] = ShackPosition.ToVector2();
        }
        
        if (ShackBounds != Rectangle.Empty) {
            tag["ShackBounds"] = ShackBounds;
        }
        
        tag["FoundShack"] = FoundOldbotShack;
    }

    public override void LoadWorldData(TagCompound tag) {
        if (tag.TryGet("ShackPosition", out Vector2 position)) {
            ShackPosition = position.ToPoint();
        }
        
        if (tag.TryGet("ShackBounds", out Rectangle bounds)) {
            ShackBounds = bounds;
        }
        
        if (tag.TryGet("FoundShack", out bool found)) {
            FoundOldbotShack = found;
        }
    }

    public override void PreUpdatePlayers() {
        if (!FoundOldbotShack && Main.LocalPlayer.Distance(ShackPosition.ToVector2() * 16f) < 830) {
            FoundOldbotShack = true;
        }
        
        var detectionRect1 = new Rectangle(PointsOfInterestSystem.ShackPosition.X + 11, PointsOfInterestSystem.ShackPosition.Y, 18, 12);
        var detectionRect2 = new Rectangle(PointsOfInterestSystem.ShackPosition.X, PointsOfInterestSystem.ShackPosition.Y + 3, 11, 9);

        LocalPlayerInShack = Main.LocalPlayer.Hitbox.Intersects(detectionRect1.ToWorldCoordinates()) 
                             || Main.LocalPlayer.Hitbox.Intersects(detectionRect2.ToWorldCoordinates());
    }

    public override void ClearWorld() {
        FoundOldbotShack = false;
        LocalPlayerInShack = false;
        ShackPosition = Point.Zero;
        ShackBounds = Rectangle.Empty;
    }
}