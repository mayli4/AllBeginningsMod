using AllBeginningsMod.Common;

namespace AllBeginningsMod.Utilities;

internal static class PlayerExtensions {
    public static void Rotate(this Player player, float rotation, Vector2? origin = null) {
        player.GetModPlayer<CollisionPlayer>().rotation = rotation;

        player.fullRotation = rotation;
        player.fullRotationOrigin = origin ?? player.fullRotationOrigin;
    }
    
    public static bool FallThrough(this Player player) => player.GetModPlayer<CollisionPlayer>().FallThrough();
}