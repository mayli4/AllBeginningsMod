using System;

namespace AllBeginningsMod.Utilities;

public readonly struct GraphicsDeviceSnapshot(GraphicsDevice graphicsDevice) : IEquatable<GraphicsDeviceSnapshot> {
    public BlendState BlendState { get; init; } = graphicsDevice.BlendState;
    public DepthStencilState DepthStencilState { get; init; } = graphicsDevice.DepthStencilState;
    public SamplerState SamplerState { get; init; } = graphicsDevice.SamplerStates[0];
    public RasterizerState RasterizerState { get; init; } = graphicsDevice.RasterizerState;

    public bool Equals(GraphicsDeviceSnapshot other) {
        return BlendState == other.BlendState &&
               SamplerState == other.SamplerState &&
               DepthStencilState == other.DepthStencilState &&
               RasterizerState == other.RasterizerState;
    }

    public override bool Equals(object obj) {
        return obj is GraphicsDeviceSnapshot other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(BlendState, SamplerState, DepthStencilState, RasterizerState);
    }

    public static bool operator ==(GraphicsDeviceSnapshot left, GraphicsDeviceSnapshot right) {
        return left.Equals(right);
    }

    public static bool operator !=(GraphicsDeviceSnapshot left, GraphicsDeviceSnapshot right) {
        return !left.Equals(right);
    }
}