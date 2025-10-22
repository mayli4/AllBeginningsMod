using System;
using Terraria.GameContent.Drawing;

namespace AllBeginningsMod.Common;

internal sealed class TileSwaying : ModSystem {
    public double TreeWindCounter { get; private set; }

    public static TileSwaying Instance;

    public static event Action PreUpdateWind;

    public override void Load() {
        Instance = this;
        On_TileDrawing.Update += On_TileDrawingOnUpdate;
    }

    public override void Unload() {
        On_TileDrawing.Update -= On_TileDrawingOnUpdate;
    }

    private void On_TileDrawingOnUpdate(On_TileDrawing.orig_Update orig, TileDrawing self) {
        orig(self);

        if(Main.dedServ)
            return;

        PreUpdateWind?.Invoke();

        double num = Math.Abs(Main.WindForVisuals);
        num = Utils.GetLerpValue(0.08f, 1.2f, (float)num, clamped: true);

        TreeWindCounter += 0.0041666666666666666 + 0.0041666666666666666 * num * 2.0;
    }
}