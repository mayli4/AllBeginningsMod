using System;

namespace AllBeginningsMod.Utilities;

#nullable enable

internal readonly struct SpriteBatchSnapshot {
    public SpriteSortMode SortMode { get; init; }
    public BlendState BlendState { get; init; } = null!;
    public SamplerState SamplerState { get; init; }
    public DepthStencilState DepthStencilState { get; init; } = null!;
    public RasterizerState RasterizerState { get; init; }
    public Effect? CustomEffect { get; init; }
    public Matrix TransformMatrix { get; init; }

    public SpriteBatchSnapshot() {
        SortMode = default;
        BlendState = default!;
        SamplerState = Main.DefaultSamplerState;
        DepthStencilState = default!;
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
}