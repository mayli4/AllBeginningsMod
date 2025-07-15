using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Immutable;
using Terraria;

namespace AllBeginningsMod.Common.PrimitiveDrawing; 

internal class DefaultTrailStyle : ITrailStyle {
    public int VertexCount(int trailPositionCount) => trailPositionCount * 2;

    public int IndexCount(int trailPositionCount) => (trailPositionCount - 1) * 6;

    public void SetBuffers(ImmutableArray<Vector2> trailPositions, Func<float, float> trailWidth, Func<float, Color> trailColor, ref VertexPositionColorTexture[] vertices, ref ushort[] indices) {
        Color color = trailColor(0f);
        Vector2 vertexOffset = trailPositions[0].DirectionTo(trailPositions[1]).RotatedBy(MathHelper.PiOver2) * trailWidth(0f) * 0.5f;
        vertices[0] = new VertexPositionColorTexture((trailPositions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
        vertices[1] = new VertexPositionColorTexture((trailPositions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

        for(int i = 1; i < trailPositions.Length; i++) {
            float factor = i / (trailPositions.Length - 1f);

            color = trailColor(factor);
            vertexOffset = trailPositions[i - 1].DirectionTo(trailPositions[i]).RotatedBy(MathHelper.PiOver2) * trailWidth(factor) * 0.5f;
            vertices[i * 2] = new VertexPositionColorTexture((trailPositions[i] - vertexOffset).ToVector3(), color, new Vector2(factor, 0f));
            vertices[i * 2 + 1] = new VertexPositionColorTexture((trailPositions[i] + vertexOffset).ToVector3(), color, new Vector2(factor, 1f));

            indices[(i - 1) * 6] = (ushort)((i - 1) * 2);
            indices[(i - 1) * 6 + 1] = (ushort)((i - 1) * 2 + 2);
            indices[(i - 1) * 6 + 2] = (ushort)((i - 1) * 2 + 3);
            indices[(i - 1) * 6 + 3] = (ushort)((i - 1) * 2 + 3);
            indices[(i - 1) * 6 + 4] = (ushort)((i - 1) * 2 + 1);
            indices[(i - 1) * 6 + 5] = (ushort)((i - 1) * 2);
        }
    }
}