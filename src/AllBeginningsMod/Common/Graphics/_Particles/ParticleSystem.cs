using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

using SVec2 = System.Numerics.Vector2;
using TML = Terraria.ModLoader;

namespace AllBeginningsMod.Common.Graphics;

[Autoload(Side = ModSide.Client)]
internal unsafe sealed class ParticleSystem : ModSystem {
    [StructLayout(LayoutKind.Auto)]
    struct Data() {
        public static readonly Data Invalid = default;
        public static readonly Vector2 DefaultGravity = new Vector2();

        public float Rotation;
        public Vector2 Position;
        public Vector2 Velocity;
    }

    private const int bits_per_mask = sizeof(ulong) * 8;

    private static ulong[] presenceMask = Array.Empty<ulong>();
    private static Data[] particles = Array.Empty<Data>();
}