using AllBeginningsMod.Common.Config;

namespace AllBeginningsMod.Common.Camera;

internal struct CameraModifierDescription() {
    public float? Zoom { get; set; } = null;
}

internal sealed class CameraSystem {
    bool _shakeEnabled = ClientConfig.Instance.EnableScreenshake;
}