using System;

namespace AllBeginningsMod.Utilities;

public readonly struct SpriteBatchSnapshot : IEquatable<GraphicsDeviceSnapshot> {
    public SpriteSortMode SortMode { get; init; }
    public BlendState BlendState { get; init; }
    public SamplerState SamplerState { get; init; }
    public DepthStencilState DepthStencilState { get; init; }
    public RasterizerState RasterizerState { get; init; }
    public Effect? CustomEffect { get; init; }
    public Matrix TransformMatrix { get; init; }

    public SpriteBatchSnapshot() {
        SortMode = default;
        BlendState = default;
        SamplerState = Main.DefaultSamplerState;
        DepthStencilState = default;
        RasterizerState = Main.Rasterizer;
        CustomEffect = null;
        TransformMatrix = Main.GameViewMatrix.TransformationMatrix;
    }

    public SpriteBatchSnapshot(SpriteBatch spriteBatch) {
        SortMode = spriteBatch.sortMode;
        BlendState = spriteBatch.blendState;
        SamplerState = spriteBatch.samplerState;
        DepthStencilState = spriteBatch.depthStencilState;
        RasterizerState = spriteBatch.rasterizerState;
        CustomEffect = spriteBatch.customEffect;
        TransformMatrix = spriteBatch.transformMatrix;
    }

    public bool Equals(GraphicsDeviceSnapshot other) {
        throw new NotImplementedException();
    }

    public override bool Equals(object obj) {
        return obj is GraphicsDeviceSnapshot other && Equals(other);
    }

    public static bool operator ==(SpriteBatchSnapshot left, SpriteBatchSnapshot right) {
        return left.Equals(right);
    }

    public static bool operator !=(SpriteBatchSnapshot left, SpriteBatchSnapshot right) {
        return !left.Equals(right);
    }

    public override int GetHashCode() {
        return HashCode.Combine((int)SortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, CustomEffect, TransformMatrix);
    }
}