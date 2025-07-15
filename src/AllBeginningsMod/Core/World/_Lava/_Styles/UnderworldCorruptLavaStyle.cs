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

public class UnderworldCorruptModLava : ModLavaStyle {
    public override string LavaTexturePath => Assets.Assets.Textures.Lavas.KEY_UnderworldCorruptLava;

    public override string BlockTexturePath => LavaTexturePath + "_Block";

    public override string SlopeTexturePath => LavaTexturePath + "_Slope";

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("AllBeginningsMod/UnderworldCorruptLavafall").Slot;

    public override int GetSplashDust() => DustID.CursedTorch;

    public override int GetDropletGore() => 0;

    public override int DebuffType() => BuffID.CursedInferno;

    public override void SelectLightColor(ref Color initialLightColor) {
        initialLightColor = Color.Yellow;
        initialLightColor.A = 255;
    }
    
    public override void ModifyVertexColors(int x, int y, ref VertexColors colors) {
        var bottomColor = Color.Yellow;
        var topColor = new Color(170, 143, 36, 255);

        int topOfActualLiquidSurfaceY = y;
        int solidTilesPassedAboveLiquid = 0;

        int currentScanY = y;
        const int max_scan_range = 250; 
        const int max_solid_tiles_per_scan = 5;

        for (int i = 0; i < max_scan_range; i++) {

            Tile scanTile = Main.tile[x, currentScanY];

            if (scanTile.LiquidType == LiquidID.Lava && scanTile.LiquidAmount > 0) {
                solidTilesPassedAboveLiquid = 0;
                topOfActualLiquidSurfaceY = currentScanY;
            } else if (scanTile.HasTile) {
                // found a solid tile thats not lava, increment passed solid tiles
                solidTilesPassedAboveLiquid++;
                if (solidTilesPassedAboveLiquid > max_solid_tiles_per_scan) {
                    // max solid  tiles passthrogh exceeded, top of pool found
                    break;
                }
            } else {
                // top of pool found
                break;
            }

            currentScanY--; // move one tile up for next iteration
        }

        float potencyFactor = 1f - ((float)solidTilesPassedAboveLiquid / (max_solid_tiles_per_scan + 1));
        potencyFactor = MathHelper.Clamp(potencyFactor, 0f, 1f);
        
        var effectiveTopColor = Color.Lerp(bottomColor, topColor, potencyFactor);
        
        float maxGradientPixelDepth = 3 * 100;

        float tileTopYPixel = y * 16f;
        float tileBottomYPixel = (y + 1) * 16f;

        float poolTopPixel = topOfActualLiquidSurfaceY * 16f;
        float depthFactorForTopVertices = (tileTopYPixel - poolTopPixel) / maxGradientPixelDepth;
        float depthFactorForBottomVertices = (tileBottomYPixel - poolTopPixel) / maxGradientPixelDepth;

        float lerpFactorForTop = 1f - depthFactorForTopVertices;
        float lerpFactorForBottom = 1f - depthFactorForBottomVertices;

        lerpFactorForTop = MathHelper.Clamp(lerpFactorForTop, 0f, 1f);
        lerpFactorForBottom = MathHelper.Clamp(lerpFactorForBottom, 0f, 1f);

        byte originalTopLeftAlpha = colors.TopLeftColor.A;
        byte originalTopRightAlpha = colors.TopRightColor.A;
        byte originalBottomLeftAlpha = colors.BottomLeftColor.A;
        byte originalBottomRightAlpha = colors.BottomRightColor.A;

        // lerpolate
        var finalColorForTop = Color.Lerp(bottomColor, effectiveTopColor, lerpFactorForTop);
        var  finalColorForBottom = Color.Lerp(bottomColor, effectiveTopColor, lerpFactorForBottom);

        colors.TopLeftColor = finalColorForTop;
        colors.TopRightColor = finalColorForTop;
        colors.BottomLeftColor = finalColorForBottom;
        colors.BottomRightColor = finalColorForBottom;

        colors.TopLeftColor.A = originalTopLeftAlpha;
        colors.TopRightColor.A = originalTopRightAlpha;
        colors.BottomLeftColor.A = originalBottomLeftAlpha;
        colors.BottomRightColor.A = originalBottomRightAlpha;
    }   
}