using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace AllBeginningsMod.Utilities;   

public partial class Helper {
    public delegate Vector2 CharacterDisplacementDelegate(int characterIndex);
    
    public static void DrawBox(SpriteBatch spriteBatch, Rectangle target, Color color) {
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, target, color);
    }
    
    public static Vector2 DrawColorCodedStringShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, TextSnippet[] snippets, Vector2 position, Color mulColor, float rotation, Vector2 origin, Vector2 baseScale, out int hoveredSnippet, float maxWidth, bool ignoreColors = false, int maxChars = -1)
    {
        Color black = Color.Black.MultiplyRGBA(mulColor);
        DrawColorCodedString(spriteBatch, font, snippets, position + new Vector2(0, 2), black, rotation, origin, baseScale, out hoveredSnippet, maxWidth, true, maxChars);
        DrawColorCodedString(spriteBatch, font, snippets, position + new Vector2(0, -2), black, rotation, origin, baseScale, out hoveredSnippet, maxWidth, true, maxChars);
        DrawColorCodedString(spriteBatch, font, snippets, position + new Vector2(2, 0), black, rotation, origin, baseScale, out hoveredSnippet, maxWidth, true, maxChars);
        DrawColorCodedString(spriteBatch, font, snippets, position + new Vector2(-2, 0), black, rotation, origin, baseScale, out hoveredSnippet, maxWidth, true, maxChars);

        return DrawColorCodedString(spriteBatch, font, snippets, position, mulColor, rotation, origin, baseScale, out hoveredSnippet, maxWidth, ignoreColors, maxChars);
    }

    /// <summary>
    /// This is a modified version of the vanilla DrawColorCodedString with some additional functionality
    /// </summary>
    public static Vector2 DrawColorCodedString(SpriteBatch spriteBatch, DynamicSpriteFont font, TextSnippet[] snippets, Vector2 position, Color mulColor, float rotation, Vector2 origin, Vector2 baseScale, out int hoveredSnippet, float maxWidth, bool ignoreColors = false, int maxChars = -1)
    {
        int num = -1;
        Vector2 vec = new Vector2(Main.mouseX, Main.mouseY);
        Vector2 vector = position;
        Vector2 result = vector;
        float x = font.MeasureString(" ").X;
        Color color = mulColor;
        float num2 = 1f;
        float num3 = 0f;

        int charIndex = 0;

        for (int i = 0; i < snippets.Length; i++)
        {
            TextSnippet textSnippet = snippets[i];
            textSnippet.Update();
            if (!ignoreColors)
                color = textSnippet.GetVisibleColor().MultiplyRGBA(mulColor);

            num2 = textSnippet.Scale;

            if (maxChars > charIndex || maxChars <= -1)
            {
                if (textSnippet.UniqueDraw(justCheckingString: false, out Vector2 size, spriteBatch, vector, color, baseScale.X * num2))
                {
                    if (vec.Between(vector, vector + size))
                        num = i;

                    vector.X += size.X;

                    result.X = Math.Max(result.X, vector.X);
                    continue;
                }
            }

            string[] lines = textSnippet.Text.Split('\n');
            lines = Regex.Split(textSnippet.Text, "(\n)");
            bool flag = true;
            foreach (string line in lines)
            {
                string[] words = Regex.Split(line, "( )");
                words = line.Split(' ');
                if (line == "\n")
                {
                    vector.Y += (float)font.LineSpacing * num3 * baseScale.Y;
                    vector.X = position.X;
                    result.Y = Math.Max(result.Y, vector.Y);
                    num3 = 0f;
                    flag = false;
                    continue;
                }

                for (int k = 0; k < words.Length; k++)
                {
                    if (k != 0)
                        vector.X += x * baseScale.X * num2;

                    if (maxWidth > 0f)
                    {
                        float num4 = font.MeasureString(words[k]).X * baseScale.X * num2;
                        if (vector.X - position.X + num4 > maxWidth)
                        {
                            vector.X = position.X;
                            vector.Y += (float)font.LineSpacing * num3 * baseScale.Y;
                            result.Y = Math.Max(result.Y, vector.Y);
                            num3 = 0f;
                        }
                    }

                    if (num3 < num2)
                        num3 = num2;

                    if (maxChars == -1)
                        spriteBatch.DrawString(font, words[k], vector, color, rotation, origin, baseScale * textSnippet.Scale * num2, SpriteEffects.None, 0f);
                    else if (maxChars >= charIndex)
                        spriteBatch.DrawString(font, words[k][..Math.Min(words[k].Length, maxChars - charIndex)], vector, color, rotation, origin, baseScale * textSnippet.Scale * num2, SpriteEffects.None, 0f);

                    charIndex += words[k].Length;

                    Vector2 vector2 = font.MeasureString(words[k]);
                    if (vec.Between(vector, vector + vector2))
                        num = i;

                    vector.X += vector2.X * baseScale.X * num2;
                    result.X = Math.Max(result.X, vector.X);
                }

                if (lines.Length > 1 && flag)
                {
                    vector.Y += (float)font.LineSpacing * num3 * baseScale.Y;
                    vector.X = position.X;
                    result.Y = Math.Max(result.Y, vector.Y);
                    num3 = 0f;
                }

                flag = true;
            }
        }

        hoveredSnippet = num;
        return result;
    }
    
    public static void DrawCustomBox(SpriteBatch sb, Asset<Texture2D> texture, Rectangle target, Color color, int cornerSize) {
        Texture2D tex = texture.Value;

        int centerSize = tex.Width - cornerSize * 2;

        if (target.Width < cornerSize * 2 || target.Height < cornerSize * 2)
            return;

        var sourceCorner = new Rectangle(0, 0, cornerSize, cornerSize);
        var sourceCorner1 = new Rectangle(tex.Width - cornerSize, 0, cornerSize, cornerSize);
        var sourceCorner2 = new Rectangle(tex.Width - cornerSize, tex.Height - cornerSize, cornerSize, cornerSize);
        var sourceCorner3 = new Rectangle(0, tex.Height - cornerSize, cornerSize, cornerSize);

        var sourceEdge = new Rectangle(cornerSize, 0, centerSize, cornerSize);
        var sourceEdge1 = new Rectangle(0, cornerSize, cornerSize, centerSize);
        var sourceEdge2 = new Rectangle(tex.Width - cornerSize, cornerSize, cornerSize, centerSize);
        var sourceEdge3 = new Rectangle(cornerSize, tex.Height - cornerSize, centerSize, cornerSize);

        var sourceCenter = new Rectangle(cornerSize, cornerSize, centerSize, centerSize);

        Rectangle inner = target;
        inner.Inflate(-centerSize, -centerSize);

        sb.Draw(tex, inner, sourceCenter, color);

        sb.Draw(tex, new Rectangle(target.X + cornerSize, target.Y, target.Width - cornerSize * 2, cornerSize), sourceEdge, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X, target.Y + cornerSize, cornerSize, target.Height - cornerSize * 2), sourceEdge1, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X - cornerSize + target.Width, target.Y + cornerSize, cornerSize, target.Height - cornerSize * 2), sourceEdge2, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X + cornerSize, target.Y + target.Height - cornerSize, target.Width - cornerSize * 2, cornerSize), sourceEdge3, color, 0, Vector2.Zero, 0, 0);

        sb.Draw(tex, new Rectangle(target.X, target.Y, cornerSize, cornerSize), sourceCorner, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y, cornerSize, cornerSize), sourceCorner1, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y + target.Height - cornerSize, cornerSize, cornerSize), sourceCorner2, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height - cornerSize, cornerSize, cornerSize), sourceCorner3, color, 0, Vector2.Zero, 0, 0);
    }
    
    public static int GetCharCount(TextSnippet[] snippets) {
        int count = 0;
        foreach (var snippet in snippets)
        {
            count += snippet.Text?.Length ?? 0;
        }
        return count;
    }
    
    public static float GetWidth(string text, float scale) {
        return FontAssets.MouseText.Value.MeasureString(text).X * scale;
    }
}