using AllBeginningsMod.Utilities;
using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace AllBeginningsMod.Common.Dialogue;

internal sealed class EvilNightgauntTextHandler : ITagHandler, ILoadable {
    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) {
        // string[] paramsStrings = options.Split('/');
        // float ampl = 4f;
        // float freq = 1f;
        // if (!string.IsNullOrEmpty(options)) {
        //     float.TryParse(paramsStrings[0], out ampl);
        //     if (paramsStrings.Length > 1)
        //         float.TryParse(paramsStrings[1], out freq);
        // }
        return new EvilNightgauntSnippet(text, baseColor);
    }

    void ILoadable.Load(Mod mod) {
        ChatManager.Register<EvilNightgauntTextHandler>(
        [
            "evilng"
        ]);
    }

    void ILoadable.Unload() { }
}
internal sealed class EvilNightgauntSnippet : TextSnippet {
    
    public EvilNightgauntSnippet(string text, Color baseColor) {
        Text = text;
        Color = baseColor;
    }
    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
        if (spriteBatch is null || justCheckingString) {
            size = default;
            return false;
        }

        DynamicSpriteFont font = FontAssets.MouseText.Value;

        Vector2 outSize = Vector2.Zero;
        Vector2 currentPosition = position;

        for (int i = 0; i < Text.Length; i++) {
            char c = Text[i];
            string str = c.ToString();
            Vector2 characterSize = font.MeasureString(str);

            Vector2 characterPosition = currentPosition + Vector2.UnitY;

            currentPosition.X += characterSize.X;
            outSize.X += characterSize.X;
            outSize.Y = Math.Max(outSize.Y, characterSize.Y);

            //ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, Color.Purple, 0f, Vector2.Zero, new Vector2(scale));
            
            Vector2 shadowOffset = new Vector2(2, 2) * scale;
            
            //Effect effect = Shaders.Text.NightgauntSpeak.Value;
            
            // effect.Parameters["speed"].SetValue(0.1f);
            // effect.Parameters["power"].SetValue(0.05f);
            // effect.Parameters["col"].SetValue(Color.Black.ToVector4());
            // effect.Parameters["uTime"].SetValue(Main.GameUpdateCount / 10f);
            //
            // var sp = spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with {CustomEffect = effect});
            //
            // ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition + shadowOffset, Color.Black * 0.5f, 0f, Vector2.Zero, new Vector2(scale));
            // ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, color, 0f, Vector2.Zero, new Vector2(scale));
            //
            // spriteBatch.EndBegin(sp);
            //
            // effect.Parameters["speed"].SetValue(0.1f);
            // effect.Parameters["power"].SetValue(0.05f);
            // effect.Parameters["col"].SetValue(new Vector4(0.7f, 0.04f, 0.04f, 1.0f));
            // effect.Parameters["uTime"].SetValue(Main.GameUpdateCount / 10f);
            //
            // var sp2 = spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with {CustomEffect = effect});
            //
            // ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, color, 0f, Vector2.Zero, new Vector2(scale));
            //
            // spriteBatch.EndBegin(sp2);
            
            Effect effect2 = Shaders.Text.WarpTooltip.Value;
            
            effect2.Parameters["speed"].SetValue(0.1f);
            effect2.Parameters["power"].SetValue(0.06f);
            effect2.Parameters["uTime"].SetValue(Main.GameUpdateCount / 30f);
            
            var sp2 = spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with {CustomEffect = effect2});
            
            ChatManager.DrawColorCodedString(spriteBatch, font, str, characterPosition, color, 0f, Vector2.Zero, new Vector2(scale));
            
            spriteBatch.EndBegin(sp2);
        }

        size = outSize;

        return true;
    }
}