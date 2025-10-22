using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;

namespace AllBeginningsMod.Utilities;

public static class RectangleExtensions {
    public static bool Intersects(this Rectangle rectangle, Vector2 center, float radius) {
        Vector2 hitboxCenter = rectangle.Center.ToVector2();
        float distanceX = MathF.Abs(center.X - hitboxCenter.X);
        float distanceY = MathF.Abs(center.Y - hitboxCenter.Y);
        float halfWidth = rectangle.Width / 2f;
        float halfHeight = rectangle.Height / 2f;

        if(distanceX > halfWidth + radius || distanceY > halfHeight + radius) {
            return false;
        }

        if(distanceX < halfWidth || distanceY < halfHeight) {
            return true;
        }

        float distanceSquared = MathF.Pow(distanceX - halfWidth, 2) + MathF.Pow(distanceY - halfHeight, 2);
        return distanceSquared < MathF.Pow(radius, 2);
    }

    public static bool Intersects(this Rectangle rectangle, Point center, float radius) {
        return rectangle.Intersects(center.ToVector2(), radius);
    }

    public static Rectangle SourceRectangle(this Projectile projectile) {
        Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
        int sourceHeight = texture.Height / Main.projFrames[projectile.type];
        return new Rectangle(0, projectile.frame * sourceHeight, texture.Width, sourceHeight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rectangle ToWorldCoordinates(this Rectangle rect) {
        return new Rectangle(
            rect.X * 16,
            rect.Y * 16,
            rect.Width * 16,
            rect.Height * 16
        );
    }
}