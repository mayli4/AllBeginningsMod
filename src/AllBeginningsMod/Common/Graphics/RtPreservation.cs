using Daybreak.Common.Features.Hooks;

namespace AllBeginningsMod.Common.Graphics;

internal static class RtContentPreserver {
    public static RenderTargetBinding[] GetAndPreserveMainRTs() {
        var bindings = Main.instance.GraphicsDevice.GetRenderTargets();
        ApplyToBindings(bindings);

        return bindings;
    }

    public static void ApplyToBindings(RenderTargetBinding[] bindings) {
        foreach (var binding in bindings) {
            if (binding.RenderTarget is not RenderTarget2D rt) {
                continue;
            }

            rt.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }

    [OnLoad]
    private static void Load() {
        Main.RunOnMainThread(() => {
                Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
                Main.graphics.ApplyChanges();
            }
        );
    }
}