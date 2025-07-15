using Microsoft.Xna.Framework;
using System;

namespace AllBeginningsMod.Utilities; 

/// <summary>
///     Caches given positions.
/// </summary>
public readonly struct PositionCache(int length) {
    public readonly Vector2[] Positions = new Vector2[length];
    public int Count => Positions.AsSpan().Length;

    /// <summary>
    ///     Sets all positions in this cache to a given position.
    /// </summary>
    public void SetAll(Vector2 position) {
        for(int i = 0; i < Positions.Length; i++) {
            Positions[i] = position;
        }
    }

    /// <summary>
    ///     Adds a position to the position cache.
    /// </summary>
    public void Add(Vector2 position) {
        for(int i = Positions.Length - 1; i > 0; i--) {
            Positions[i] = Positions[i - 1];
        }

        Positions[0] = position;
    }
}