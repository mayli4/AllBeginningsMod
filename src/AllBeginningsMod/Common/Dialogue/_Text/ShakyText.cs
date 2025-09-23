using ReLogic.Graphics;
using System;
using System.Globalization;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace AllBeginningsMod.Common.Dialogue;

internal sealed class ShakyHandler : ITagHandler, ILoadable {
    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) {
        Color snippetColor = baseColor;
        float strength = 1.5f;

        if (!string.IsNullOrEmpty(options)) {
            string[] parts = options.Split('/');
            
            if (parts.Length >= 1) {
                if (!string.IsNullOrEmpty(parts[0])) {
                    if (int.TryParse(parts[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result)) {
                        snippetColor = new Color((result >> 16) & 0xFF, (result >> 8) & 0xFF, result & 0xFF);
                    }
                    else {
                        if (parts.Length == 1 && float.TryParse(parts[0], CultureInfo.InvariantCulture, out var parsedStrength)) {
                            strength = parsedStrength;
                        }
                    }
                }

                if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1])) {
                    float.TryParse(parts[1], CultureInfo.InvariantCulture, out strength);
                }
            }
        }
        return new ShakySnippet(text, snippetColor, strength);
    }

    public void Load(Mod mod) {
        ChatManager.Register<ShakyHandler>(
        [
            "shake"
        ]);
    }

    public void Unload() { }
}

internal sealed class ShakySnippet : TextSnippet {
    public float Strength;
    private Vector2[] shakes;
    private uint lastShakeGUC;

    public ShakySnippet(string text, Color baseColor, float strength) {
        Text = text;
        Color = baseColor;
        Strength = strength;
        shakes = new Vector2[Text.Length];
        lastShakeGUC = 0;
    }

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
        if (spriteBatch is null || justCheckingString) {
            size = default;
            return false;
        }

        if (lastShakeGUC != Main.GameUpdateCount) {
            for (int i = 0; i < Text.Length; i++) {
                shakes[i] = Main.rand.NextVector2Circular(Strength, Strength);
            }
            lastShakeGUC = Main.GameUpdateCount;
        }

        Vector2 outSize = Vector2.Zero;
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Vector2 currentPosition = position;
        Vector2 shadowOffset = new Vector2(2, 2) * scale;

        for (int i = 0; i < Text.Length; i++) {
            char c = Text[i];
            var shake = shakes[i];
            string str = c.ToString();
            var characterSize = font.MeasureString(str);
            
            ChatManager.DrawColorCodedString(spriteBatch, font, str, currentPosition + shake + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, new Vector2(scale));

            ChatManager.DrawColorCodedString(spriteBatch, font, str, currentPosition + shake, color, 0f, Vector2.Zero, new Vector2(scale));

            currentPosition.X += characterSize.X;
            outSize.X += characterSize.X;
            outSize.Y = Math.Max(outSize.Y, characterSize.Y);
        }

        size = outSize;
        return true;
    }
}