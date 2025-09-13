using System;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Utilities;

public static partial class Helper {
    public readonly static string PlaceholderTextureKey = "Terraria/Images/Item_0";
    public static Vector2 TileOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

    public static Vector2 InitialVelocityRequiredToHitPosition(Vector2 initialPosition, Vector2 targetPosition, float gravity, float initialSpeed, bool secondAngle = false) {
        Vector2 localTargetPosition = targetPosition - initialPosition;
        localTargetPosition.X = MathF.Abs(localTargetPosition.X);
        float randomShit = MathF.Pow(initialSpeed, 4) - gravity * (gravity * MathF.Pow(localTargetPosition.X, 2) + 2f * localTargetPosition.Y * MathF.Pow(initialSpeed, 2));
        float angle = MathF.Atan(
            (MathF.Pow(initialSpeed, 2) + MathF.Sqrt(Math.Max(randomShit, 0f)) * (secondAngle ? -1 : 1))
            / (gravity * localTargetPosition.X)
        );

        Vector2 velocity = angle.ToRotationVector2() * initialSpeed;
        velocity.Y = -velocity.Y;
        velocity.X *= MathF.Sign(targetPosition.X - initialPosition.X);

        return velocity;
    }

    public static void ForEachNPCInRange(Vector2 position, float range, Action<NPC> action) {
        for(int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if(npc is null || !npc.active || !npc.Hitbox.Intersects(position, range)) {
                continue;
            }

            action(npc);
        }
    }

    public static bool HoleAtPosition(NPC npc, float xPosition) {
        int tileWidth = npc.width / 16;
        xPosition = (int)(xPosition / 16f) - tileWidth;
        if(npc.velocity.X > 0)
            xPosition += tileWidth;

        int tileY = (int)((npc.position.Y + npc.height) / 16f);
        for(int y = tileY; y < tileY + 2; y++) {
            for(int x = (int)xPosition; x < xPosition + tileWidth; x++) {
                if(Main.tile[x, y].HasTile)
                    return false;
            }
        }

        return true;
    }

    public static Point ToSafeTileCoordinates(this Vector2 vec) {
        return new Point(MathHelper.Clamp((int)vec.X >> 4, 0, Main.maxTilesX), MathHelper.Clamp((int)vec.Y >> 4, 0, Main.maxTilesY));
    }

    public static Vector2 InverseKinematic(Vector2 start, Vector2 end, float A, float B, bool flip) {
        float C = Vector2.Distance(start, end);
        float angle = (float)Math.Acos(Math.Clamp((C * C + A * A - B * B) / (2f * C * A), -1f, 1));
        if(flip)
            angle *= -1;
        return start + (angle + start.AngleTo(end)).ToRotationVector2() * A;
    }

    public static Point? RaytraceToFirstSolid(this Vector2 pos1, Vector2 pos2) {
        Point point1 = pos1.ToSafeTileCoordinates();
        Point point2 = pos2.ToSafeTileCoordinates();
        return RaytraceToFirstSolid(point1, point2);
    }

    public static Point? RaytraceToFirstSolid(this Point pos1, Point pos2) {
        return RaytraceToFirstSolid(pos1.X, pos1.Y, pos2.X, pos2.Y);
    }

    public static Point? RaytraceToFirstSolid(int x0, int y0, int x1, int y1) {
        //Bresenham's algorithm
        int horizontalDistance = Math.Abs(x1 - x0); //Delta X
        int verticalDistance = Math.Abs(y1 - y0); //Delta Y
        int horizontalIncrement = (x1 > x0) ? 1 : -1; //S1
        int verticalIncrement = (y1 > y0) ? 1 : -1; //S2

        int x = x0;
        int y = y0;
        int i = 1 + horizontalDistance + verticalDistance;
        int E = horizontalDistance - verticalDistance;
        horizontalDistance *= 2;
        verticalDistance *= 2;

        while(i > 0) {
            if(Main.tile[x, y].IsTileSolidOrPlatform())
                return new Point(x, y);

            if(E > 0) {
                x += horizontalIncrement;
                E -= verticalDistance;
            }
            else {
                y += verticalIncrement;
                E += horizontalDistance;
            }
            i--;
        }
        return null;
    }

    public static bool IsTileSolidOrPlatform(this Tile tile) => tile != null && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NormalizeAngle(float angle) {
        angle %= (2 * MathF.PI);
        if(angle > MathF.PI) angle -= 2 * MathF.PI;
        else if(angle <= -MathF.PI) angle += 2 * MathF.PI;
        return angle;
    }

    public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness) {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(
            Textures.Sample.Pixel.Value,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0
        );
    }
    
    public static float RadiusAtEllipsePoint(float horizontalSemiAxis, float verticalSemiAxis, Vector2 point) {
        float angle = AngleAtPoint(point);
        return (horizontalSemiAxis * verticalSemiAxis) / (float)Math.Sqrt(Math.Pow(horizontalSemiAxis, 2) * Math.Pow(Math.Sin(angle), 2) + Math.Pow(verticalSemiAxis, 2) * Math.Pow(Math.Cos(angle), 2));
    }
    
    public static float AngleAtPoint(Vector2 point) {
        return (float)Math.Atan2(point.Y, point.X);
    }
}