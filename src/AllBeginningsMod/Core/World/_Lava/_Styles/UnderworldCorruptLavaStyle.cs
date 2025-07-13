using AllBeginningsMod.Content.Biomes;
using AllBeginningsMod.Core.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
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

    public override void SelectLightColor(ref Color initialLightColor) {
        initialLightColor = Color.Yellow;
        initialLightColor.A = 255;
    }
    
    public override void ModifyVertexColors(int x, int y, ref VertexColors colors) {
        var bottomColor = new Color(124, 169, 19, 255);
        var topColor = new Color(Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, 255);
        int topOfLiquidInColumnY = y;
        
        //iterate upwards from current tile at y, stop if we hit a tile that is not the same liquid or has no liquid
        
        while (topOfLiquidInColumnY > 0 && Main.tile[x, topOfLiquidInColumnY - 1].LiquidType == LiquidID.Lava && Main.tile[x, topOfLiquidInColumnY - 1].LiquidAmount > 0) {
            topOfLiquidInColumnY--;
        }
        
        var gradientDepthTiles = 30f;
        var gradientDepthPixels = gradientDepthTiles * 16f;
        
        var topOfColumnYPixel = topOfLiquidInColumnY * 16f;

        var currentVertexTopYPixel = y * 16f;
        var currentVertexBottomYPixel = (y + 1) * 16f;
        
        //factor should be 1 when at the top, and 0 at top + gradientdepth, current y - top of colum gives depth in the column
        var factorForTopVertices = 1f - ((currentVertexTopYPixel - topOfColumnYPixel) / gradientDepthPixels);
        var factorForBottomVertices = 1f - ((currentVertexBottomYPixel - topOfColumnYPixel) / gradientDepthPixels);

        factorForTopVertices = MathHelper.Clamp(factorForTopVertices, 0f, 1f);
        factorForBottomVertices = MathHelper.Clamp(factorForBottomVertices, 0f, 1f);

        // lerpolate
        var finalColorForTop = Color.Lerp(bottomColor, topColor, factorForTopVertices);
        var finalColorForBottom = Color.Lerp(bottomColor, topColor, factorForBottomVertices);

        var originalAlphaTopLeft = colors.TopLeftColor.A;
        var originalAlphaTopRight = colors.TopRightColor.A;
        var originalAlphaBottomLeft = colors.BottomLeftColor.A;
        var originalAlphaBottomRight = colors.BottomRightColor.A;

        colors.TopLeftColor = new Color(finalColorForTop.R, finalColorForTop.G, finalColorForTop.B, originalAlphaTopLeft);
        colors.TopRightColor = new Color(finalColorForTop.R, finalColorForTop.G, finalColorForTop.B, originalAlphaTopRight);
        colors.BottomLeftColor = new Color(finalColorForBottom.R, finalColorForBottom.G, finalColorForBottom.B, originalAlphaBottomLeft);
        colors.BottomRightColor = new Color(finalColorForBottom.R, finalColorForBottom.G, finalColorForBottom.B, originalAlphaBottomRight);
    }   
}