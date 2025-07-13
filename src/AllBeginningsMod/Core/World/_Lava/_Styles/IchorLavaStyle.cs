using AllBeginningsMod.Content.Biomes;
using AllBeginningsMod.Core.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

public class IchorLavafall : ModWaterfallStyle {
    public override string Texture => Assets.Assets.Textures.Lavas.KEY_IchorLavafall;
}

public class IchorLava : LavaStyle {
    public override string LavaTexturePath => Assets.Assets.Textures.Lavas.KEY_IchorLava;

    public override string BlockTexturePath => LavaTexturePath + "_Block";

    public override string SlopeTexturePath => LavaTexturePath + "_Slope";

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("AllBeginningsMod/IchorLavafall").Slot;

    public override int GetSplashDust() => DustID.Ichor;

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

    public override void ModifyVertexColors(int x, int y, ref VertexColors colors) {

        var bottomColor = new Color(Color.Red.R, Color.Red.G, Color.Red.B, 255);
        var topColor = new Color(Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, 255);
        
        var tileTopYPixel = y * 16f;
        var tileBottomYPixel = (y + 1) * 16f;
        
        var screenTopPixel = Main.screenPosition.Y;
        var screenBottomPixel = Main.screenPosition.Y + Main.screenHeight;
        var factorForTopVertices = 1f - ((tileTopYPixel - screenTopPixel) / Main.screenHeight);
        var factorForBottomVertices = 1f - ((tileBottomYPixel - screenTopPixel) / Main.screenHeight);

        factorForTopVertices = MathHelper.Clamp(factorForTopVertices, 0f, 1f);
        factorForBottomVertices = MathHelper.Clamp(factorForBottomVertices, 0f, 1f);

        var finalColorForTop = Color.Lerp(bottomColor, topColor, factorForTopVertices);
        var finalColorForBottom = Color.Lerp(bottomColor, topColor, factorForBottomVertices);
        colors.TopLeftColor = finalColorForTop;
        colors.TopRightColor = finalColorForTop;
        colors.BottomLeftColor = finalColorForBottom;
        colors.BottomRightColor = finalColorForBottom;
    }   
}