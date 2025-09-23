﻿using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

namespace AllBeginningsMod.Common.Dialogue;

internal sealed class WavyHandler : ITagHandler, ILoadable {
    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) {
        string[] paramsStrings = options.Split('/');
        float ampl = 4f;
        float freq = 1f;
        if (!string.IsNullOrEmpty(options))
        {
            float.TryParse(paramsStrings[0], out ampl);
            if (paramsStrings.Length > 1)
                float.TryParse(paramsStrings[1], out freq);
        }
        return new WavySnippet(text, baseColor, ampl, freq);
    }

    void ILoadable.Load(Mod mod) {
        ChatManager.Register<WavyHandler>(
        [
            "wavy"
        ]);
    }

    void ILoadable.Unload() { }
}
internal sealed class WavySnippet : TextSnippet {
    public float Amplitude;
    public float Frequency;
    private float[] yOffsets;
    private uint lastpreComputeGUC;
    
    public WavySnippet(string text, Color baseColor, float amplitude, float frequency) {
        Text = text;
        Color = baseColor;
        Amplitude = amplitude;
        Frequency = frequency;
        yOffsets = new float[Text.Length];
        lastpreComputeGUC = 0;
    }
    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
        if (spriteBatch is null || justCheckingString) {
            size = default;
            return false;
        }

        DynamicSpriteFont font = FontAssets.MouseText.Value;

        // to have consistent yOffsets for every character, even when drawn with shadows, we can pre-compute new yOffsets for each character every frame.
        if (lastpreComputeGUC != Main.GameUpdateCount) {
            Vector2 tempPos = Vector2.Zero;
            double time = Main.timeForVisualEffects / 32d;

            for (int i = 0; i < Text.Length; i++) {
                char c = Text[i];
                string str = c.ToString();
                Vector2 charSize = font.MeasureString(str);

                if (char.IsWhiteSpace(c)) {
                    yOffsets[i] = 0;
                }
                else {
                    float posOffset = tempPos.X / 16f;
                    yOffsets[i] = MathF.Sin(((float)time + posOffset) * Frequency) * Amplitude;
                }
                tempPos.X += charSize.X;
            }
            lastpreComputeGUC = Main.GameUpdateCount;
        }

        Vector2 outSize = Vector2.Zero;
        Vector2 currentPosition = position;

        for (int i = 0; i < Text.Length; i++) {
            char c = Text[i];
            string str = c.ToString();
            Vector2 characterSize = font.MeasureString(str);

            Vector2 characterPosition = currentPosition + Vector2.UnitY * yOffsets[i];

            currentPosition.X += characterSize.X;
            outSize.X += characterSize.X;
            outSize.Y = Math.Max(outSize.Y, characterSize.Y);

            ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, color, 0f, Vector2.Zero, new Vector2(scale));
        }

        size = outSize;

        return true;
    }
}