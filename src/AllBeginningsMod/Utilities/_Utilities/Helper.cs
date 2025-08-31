using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace AllBeginningsMod.Utilities;

public static partial class Helper {
    public readonly static string PlaceholderTextureKey = "Terraria/Images/Item_0";

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
}