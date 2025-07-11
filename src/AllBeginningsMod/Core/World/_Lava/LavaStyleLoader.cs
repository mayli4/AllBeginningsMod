using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;

namespace AllBeginningsMod.Core.World;

public class LavaStyleLoader : ModSystem {
    internal static List<LavaStyle> CustomLavaStyles = new List<LavaStyle>();
    internal static Texture2D LavaBlockTexture;
    internal static Texture2D LavaTexture;
    internal static Texture2D LavaSlopeTexture;
    
    private static LavaStyle _cachedLavaStyle;

    private static readonly MethodInfo textureGetValueMethod = typeof(Asset<Texture2D>).GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);

    public static void RegisterStyle(LavaStyle lavaStyle) {
        lavaStyle.Type = CustomLavaStyles.Count;
        CustomLavaStyles.Add(lavaStyle);
    }

    public override void Load() {
        CustomLavaStyles = new List<LavaStyle>();

        if (Main.netMode != NetmodeID.Server) {
            LavaBlockTexture = ModContent.Request<Texture2D>("Terraria/Images/Liquid_1", AssetRequestMode.ImmediateLoad).Value;
            LavaTexture = ModContent.Request<Texture2D>("Terraria/Images/Misc/water_1", AssetRequestMode.ImmediateLoad).Value;
            LavaSlopeTexture = ModContent.Request<Texture2D>("Terraria/Images/LiquidSlope_1", AssetRequestMode.ImmediateLoad).Value;
        }
    }

    public override void Unload() {
        if (CustomLavaStyles != null)
            foreach (LavaStyle lavaStyle in CustomLavaStyles)
                lavaStyle?.Unload();

        CustomLavaStyles = null;
        LavaBlockTexture = null;
        LavaTexture = null;
    }
    
    public override void OnModLoad() {
        On_TileDrawing.DrawPartialLiquid += DrawCustomLava;
        On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += DrawCustomLavafalls;
        On_Main.RenderWater += CacheLavaStyle;
        IL_LiquidRenderer.DrawNormalLiquids += ChangeWaterQuadColors;
        On_TileLightScanner.ApplyLiquidLight += On_TileLightScanner_ApplyLiquidLight;
        // lava splash dusts change
        IL_Item.MoveInWorld += ChangeItemLavaSplashDust;
        IL_Player.Update += ChangePlayerLavaSplashDust;
        IL_Projectile.Update += ChangeProjectileLavaSplashDust;
        IL_NPC.Collision_WaterCollision += ChangeNPCLavaSplashDust;
    }
    // these are made into separate methods so if the stuff ever changes per entity type its a bit easier to change
    private void ChangeNPCLavaSplashDust(ILContext il) {
        ILCursor c = new ILCursor(il);
        for(int i = 0; i < 2; i++) {
            c.GotoNext(MoveType.After,
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                        i => i.MatchLdcI4(12),
                        i => i.MatchAdd(),
                        i => i.MatchLdcI4(24),
                        i => i.MatchLdcI4(35));
            c.EmitPop();
            c.EmitDelegate(() =>
            {
                if(_cachedLavaStyle != null)
                    return _cachedLavaStyle.GetSplashDust();
                else return DustID.Lava;
            });
        }
    }
    private void ChangePlayerLavaSplashDust(ILContext il) {
        ILCursor c = new ILCursor(il);
        for(int i = 0; i < 2; i++) {
            c.GotoNext(MoveType.After,
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                        i => i.MatchLdcI4(12),
                        i => i.MatchAdd(),
                        i => i.MatchLdcI4(24),
                        i => i.MatchLdcI4(35));
            c.EmitPop();
            c.EmitDelegate(() =>
            {
                if(_cachedLavaStyle != null)
                    return _cachedLavaStyle.GetSplashDust();
                else return DustID.Lava;
            });
        }
    }

    private void ChangeItemLavaSplashDust(ILContext il) {
        ILCursor c = new ILCursor(il);
        for(int i = 0; i < 2; i++) {
            c.GotoNext(MoveType.After,
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                        i => i.MatchLdcI4(12),
                        i => i.MatchAdd(),
                        i => i.MatchLdcI4(24),
                        i => i.MatchLdcI4(35));
            c.EmitPop();
            c.EmitDelegate(() =>
            {
                if(_cachedLavaStyle != null)
                    return _cachedLavaStyle.GetSplashDust();
                else return DustID.Lava;
            });
        }
    }

    private void ChangeProjectileLavaSplashDust(ILContext il) {
        ILCursor c = new ILCursor(il);
        for (int i = 0; i < 2; i++) {
            c.GotoNext(MoveType.After,
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld(typeof(Entity).GetField("width", BindingFlags.Instance | BindingFlags.Public)),
                        i => i.MatchLdcI4(12),
                        i => i.MatchAdd(),
                        i => i.MatchLdcI4(24),
                        i => i.MatchLdcI4(35));
            c.EmitPop();
            c.EmitDelegate(() =>
            {
                if(_cachedLavaStyle != null)
                    return _cachedLavaStyle.GetSplashDust();
                else return DustID.Lava;
            });
        }
    }

    internal static int SelectLavafallStyle(int initialLavafallStyle) {
        if (initialLavafallStyle != 1)
            return initialLavafallStyle;

        if (_cachedLavaStyle != default) {
            int waterfallStyle = _cachedLavaStyle.ChooseWaterfallStyle();
            if (waterfallStyle >= 0)
                return waterfallStyle;
        }

        return initialLavafallStyle;
    }

    internal static Color SelectLavafallColor(int initialLavafallStyle, Color initialLavafallColor) {
        if (initialLavafallStyle != 1)
            return initialLavafallColor;

        if (_cachedLavaStyle != default) {
            _cachedLavaStyle.SelectLightColor(ref initialLavafallColor);
            return initialLavafallColor;
        }

        return initialLavafallColor;
    }
        
    public static LavaStyle Get(int type) => CustomLavaStyles[type];

    private static void CacheLavaStyle(On_Main.orig_RenderWater orig, Main self) {
        _cachedLavaStyle = default;

        foreach (ModBiome biome in ModContent.GetContent<ModBiome>()) {
            if (biome is IHasCustomLavaBiome customLavaBiome && Main.LocalPlayer.InModBiome(biome)) {
                _cachedLavaStyle = customLavaBiome.LavaStyle;
                break;
            }
        }

        orig(self);
    }

    private static void DrawCustomLava(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors) {
        if (liquidType != 1) {
            orig(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
            return;
        }

        int slope = (int)tileCache.Slope;
        colors = SelectLavaQuadColor(TextureAssets.LiquidSlope[liquidType].Value, ref colors, true);
        
        if (!TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType] || behindBlocks || slope == 0) {
            Texture2D liquidTexture = SelectLavaTexture(LavaStyleLoader.LavaBlockTexture, LiquidTileType.Block);
            Main.tileBatch.Draw(liquidTexture, position, liquidSize, colors, default(Vector2), 1f, SpriteEffects.None);
            return;
        }

        Texture2D slopeTexture = SelectLavaTexture(LavaStyleLoader.LavaSlopeTexture, LiquidTileType.Slope);
        liquidSize.X += 18 * (slope - 1);
        switch (slope) {
            case 1:
                Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
            case 2:
                Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
            case 3:
                Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
            case 4:
                Main.tileBatch.Draw(slopeTexture, position, liquidSize, colors, Vector2.Zero, 1f, SpriteEffects.None);
                break;
        }
    }

    private static void ChangeWaterQuadColors(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        cursor.TryGotoNext(c => c.MatchLdfld<LiquidRenderer>("_liquidTextures"));
            
        // locate liquid tex value
        cursor.TryGotoNext(MoveType.After, c => c.MatchCallvirt(textureGetValueMethod));
        
        cursor.EmitDelegate<Func<Texture2D, Texture2D>>(initialTexture => SelectLavaTexture(initialTexture, LiquidTileType.Fall));

        // locate liquid light color
        cursor.TryGotoNext(MoveType.After, c => c.MatchLdloc(9));

        // dont fuck with non liquid textures!
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, typeof(LiquidRenderer).GetField("_liquidTextures")!);
        cursor.Emit(OpCodes.Ldloc, 8);
        cursor.Emit(OpCodes.Ldelem_Ref);
        cursor.Emit(OpCodes.Ldloc, 8);
        cursor.Emit(OpCodes.Ldloc, 3);
        cursor.Emit(OpCodes.Ldloc, 4);

        cursor.EmitDelegate<Func<VertexColors, Texture2D, int, int, int, VertexColors>>((initialColor, initialTexture, liquidType, x, y) => {
            if (_cachedLavaStyle != default) {
                initialColor = SelectLavaQuadColor(initialTexture, ref initialColor, liquidType == 1);
            }

            return initialColor;
        });
    }

    private static void DrawCustomLavafalls(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects) {
        waterfallType = LavaStyleLoader.SelectLavafallStyle(waterfallType);
        color = LavaStyleLoader.SelectLavafallColor(waterfallType, color);

        orig(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
    }
        
    private static Texture2D SelectLavaTexture(Texture2D initialTexture, LiquidTileType type) {
        if (initialTexture != LavaStyleLoader.LavaTexture &&
            initialTexture != LavaStyleLoader.LavaBlockTexture &&
            initialTexture != LavaStyleLoader.LavaSlopeTexture)
            return initialTexture;

        if (_cachedLavaStyle == default)
            return initialTexture;

        switch (type)
        {
            case LiquidTileType.Block:
                return _cachedLavaStyle.BlockTexture;
            case LiquidTileType.Fall:
                return _cachedLavaStyle.LavaTexture;
            case LiquidTileType.Slope:
                return _cachedLavaStyle.SlopeTexture;
        }

        return initialTexture;
    }

    private static VertexColors SelectLavaQuadColor(Texture2D initialTexture, ref VertexColors initialColor, bool forceTrue = false) {
        if (!forceTrue) {
            if (initialTexture != LavaStyleLoader.LavaTexture &&
                initialTexture != LavaStyleLoader.LavaBlockTexture &&
                initialTexture != LavaStyleLoader.LavaSlopeTexture)
                return initialColor;
        }

        if (_cachedLavaStyle == default)
            return initialColor;

        _cachedLavaStyle.SelectLightColor(ref initialColor.TopLeftColor);
        _cachedLavaStyle.SelectLightColor(ref initialColor.TopRightColor);
        _cachedLavaStyle.SelectLightColor(ref initialColor.BottomLeftColor);
        _cachedLavaStyle.SelectLightColor(ref initialColor.BottomRightColor);
        return initialColor;
    }
    
    private void On_TileLightScanner_ApplyLiquidLight(On_TileLightScanner.orig_ApplyLiquidLight orig, TileLightScanner self, Tile tile, ref Vector3 lightColor) {
        orig.Invoke(self, tile, ref lightColor);
        if (tile.LiquidType == LiquidID.Lava && _cachedLavaStyle != default) {
            var currentLightColor = new Color(lightColor.X, lightColor.Y, lightColor.Z);
            _cachedLavaStyle.SelectLightColor(ref currentLightColor);
            lightColor = new Vector3(currentLightColor.R / 255f, currentLightColor.G / 255f, currentLightColor.B / 255f);
        }
    }
}