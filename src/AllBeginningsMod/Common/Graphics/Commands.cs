using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AllBeginningsMod.Common.Graphics;
public struct Commands() {
    public List<CommandType> Types = [];
    public List<int> Datas = [];

    public readonly int Count => Types.Count;
    public readonly void Add(CommandType type, int data) {
        Types.Add(type);
        Datas.Add(data);
    }

    public readonly void AddRange(in Commands commands) {
        Types.AddRange(commands.Types);
        Datas.AddRange(commands.Datas);
    }

    public readonly void Clear() {
        Types.Clear();
        Datas.Clear();
    }
}

public enum CommandType : byte {
    DrawTrail,
    DrawSprite,

    Begin,
    End,

    ApplyEffect,
    EffectParams,

    SetBlendState,
    SetTexture,
    SetSamplerState,
}

public record struct SamplerStateData(int Index, SamplerState State);
public record struct TextureData(int Index, Texture2D Texture);

public record struct DrawSpriteData(
    Texture2D Texture,
    Color Color,
    Matrix Matrix,
    Vector4 Source,
    Effect Effect
);

public record struct DrawTrailData(
    int PositionsIndex,
    int PositionCount,
    Func<float, float> Width,
    Func<float, Color> Color,
    int EffectDataIndex
);

public record struct BeginData(float Scale);
public record struct EffectData(Effect Effect, int ParameterIndex, int ParameterCount);
public record struct EffectParameterData(int Index, ParameterValue Value);

[StructLayout(LayoutKind.Explicit)]
public struct ParameterValue {
    [FieldOffset(0)]
    public ParameterValueType Type;

    [FieldOffset(8)]
    public Texture2D Texture2D;

    [FieldOffset(16)]
    public float Float;

    [FieldOffset(16)]
    public int Int;

    [FieldOffset(16)]
    public Vector2 Vector2;

    [FieldOffset(16)]
    public Vector3 Vector3;

    [FieldOffset(16)]
    public Vector4 Vector4;

    [FieldOffset(16)]
    public Matrix Matrix;

    public static implicit operator ParameterValue(float value) => new()
    {
        Type = ParameterValueType.Float,
        Float = value,
    };

    public static implicit operator ParameterValue(int value) => new()
    {
        Type = ParameterValueType.Int,
        Int = value,
    };

    public static implicit operator ParameterValue(Vector2 value) => new()
    {
        Type = ParameterValueType.Vector2,
        Vector2 = value,
    };

    public static implicit operator ParameterValue(Vector3 value) => new()
    {
        Type = ParameterValueType.Vector3,
        Vector3 = value,
    };

    public static implicit operator ParameterValue(Vector4 value) => new()
    {
        Type = ParameterValueType.Vector4,
        Vector4 = value,
    };

    public static implicit operator ParameterValue(Texture2D value) => new()
    {
        Type = ParameterValueType.Texture2D,
        Texture2D = value,
    };

    public static implicit operator ParameterValue(Matrix value) => new()
    {
        Type = ParameterValueType.Matrix,
        Matrix = value,
    };
}

public enum ParameterValueType {
    Float,
    Int,
    Vector2,
    Vector3,
    Vector4,
    Texture2D,
    Matrix,
}
