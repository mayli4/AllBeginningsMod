﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common.Rendering;

// adapted from scalie, weird and ill make a better system for it laterrr

public class FoliageBlending : ModSystem {
    public static List<DrawData> OverTilesData = [];
    public static List<DrawData> UnderTilesData = [];
    
    public static BlendState CeilBlend = new() {
        AlphaBlendFunction = BlendFunction.Max,
        ColorBlendFunction = BlendFunction.Max,
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One
    };
    
    public override void Load() {
        On_Main.DoDraw_WallsTilesNPCs += DrawUnderTileLayer;
    }
    
    private void DrawUnderTileLayer(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self) {
        if (UnderTilesData.Count > 0) {
            DrawBlacks(Main.spriteBatch, UnderTilesData);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(default, CeilBlend, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
            DrawReals(Main.spriteBatch, UnderTilesData);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

            UnderTilesData.Clear();
        }

        orig(self);
    }

    public override void PostDrawTiles() {
        if (OverTilesData.Count > 0) {
            Main.spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
            DrawBlacks(Main.spriteBatch, OverTilesData);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(default, CeilBlend, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
            DrawReals(Main.spriteBatch, OverTilesData);
            Main.spriteBatch.End();

            OverTilesData.Clear();
        }
    }

    private void DrawBlacks(SpriteBatch spriteBatch, List<DrawData> datas) {
        foreach (var data in datas) {
            var black = data;
            black.color = Color.Black;
            black.position -= Main.screenPosition;
            black.Draw(spriteBatch);
        }
    }

    private void DrawReals(SpriteBatch spriteBatch, List<DrawData> datas) {
        foreach (var data in datas) {
            var over = data;
            over.position -= Main.screenPosition;
            over.Draw(spriteBatch);
        }
    }
}