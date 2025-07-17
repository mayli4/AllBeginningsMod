using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Immutable;

namespace AllBeginningsMod.Common.PrimitiveDrawing; 

public interface ITrailStyle {
    int VertexCount(int trailPositionCount);
    int IndexCount(int trailPositionCount);
    /// <summary>
    /// Use this function to set the vertices and indices.
    /// </summary>
    void SetBuffers(ImmutableArray<Vector2> trailPositions, Func<float, float> trailWidth, Func<float, Color> trailColor, ref VertexPositionColorTexture[] vertices, ref ushort[] indices);
}