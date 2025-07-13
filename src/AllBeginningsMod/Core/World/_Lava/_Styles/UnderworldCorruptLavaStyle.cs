using AllBeginningsMod.Content.Biomes;
using AllBeginningsMod.Core.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

public class UnderworldCorruptLavafall : ModWaterfallStyle {
    public override string Texture => Assets.Assets.Textures.Lavas.KEY_UnderworldCorruptLavafall;
}

public class UnderworldCorruptLava : LavaStyle {
    public override string LavaTexturePath => Assets.Assets.Textures.Lavas.KEY_UnderworldCorruptLava;

    public override string BlockTexturePath => LavaTexturePath + "_Block";

    public override string SlopeTexturePath => LavaTexturePath + "_Slope";

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("AllBeginningsMod/UnderworldCorruptLavafall").Slot;

    public override int GetSplashDust() => DustID.CursedTorch;

    public override int GetDropletGore() => 0;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        r = 0.0f;
        g = 0.0f;
        b = 2.0f;
    }

    public override void SelectLightColor(ref Color initialLightColor) {
        initialLightColor = Color.Yellow;
        initialLightColor.A = 255;
    }
}