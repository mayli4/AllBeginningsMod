using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace AllBeginningsMod.Utilities;

public readonly struct SpriteBatchSnapshot(SpriteBatch spriteBatch) {
    public SpriteSortMode SortMode { get; init; } = spriteBatch.sortMode;
    public BlendState BlendState { get; init; } = spriteBatch.blendState;
    public SamplerState SamplerState { get; init; } = spriteBatch.samplerState;
    public DepthStencilState DepthStencilState { get; init; } = spriteBatch.depthStencilState;
    public RasterizerState RasterizerState { get; init; } = spriteBatch.rasterizerState;
    public Effect? CustomEffect { get; init; } = spriteBatch.customEffect;
    public Matrix TransformMatrix { get; init; } = spriteBatch.transformMatrix;
    
    public static SpriteBatchSnapshot Default() {
        return new SpriteBatchSnapshot
        {
            SortMode = default,
            BlendState = default,
            SamplerState = Main.DefaultSamplerState,
            DepthStencilState = default,
            RasterizerState = Main.Rasterizer,
            CustomEffect = null,
            TransformMatrix = Main.GameViewMatrix.TransformationMatrix
        };
    }
    
    public static SpriteBatchSnapshot Capture(SpriteBatch spriteBatch) {
        return new SpriteBatchSnapshot
        {
            SortMode = spriteBatch.sortMode,
            BlendState = spriteBatch.blendState,
            SamplerState = spriteBatch.samplerState,
            DepthStencilState = spriteBatch.depthStencilState,
            RasterizerState = spriteBatch.rasterizerState,
            CustomEffect = spriteBatch.customEffect,
            TransformMatrix = spriteBatch.transformMatrix
        };
     }
}