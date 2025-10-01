using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AllBeginningsMod.Common.Graphics;
public struct Pipeline(Graphics graphics) {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Pipeline ApplyOutline(Color color, float threshold = 0.001f) {
        ApplyEffect(
            Shaders.Fragment.Outline.Value,
            ("uColor", color.ToVector4()),
            ("uSize", Main.ScreenSize.ToVector2()),
            ("uThreshold", threshold)
        );
        return this;
    }

    public readonly Pipeline DrawTrail(
        ReadOnlySpan<Vector2> positions,
        Func<float, float> width,
        Func<float, Color> color,
        Effect effect,
        params ReadOnlySpan<(string, ParameterValue)> parameters
    ) {
        var effectDataIndex = AddEffectData(effect, parameters);

        var trailPositionsIndex = graphics.PositionDatas.Count;
        graphics.PositionDatas.AddRange(positions);

        var trailDataIndex = graphics.TrailDatas.Count;
        graphics.TrailDatas.Add(new()
        {
            PositionsIndex = trailPositionsIndex,
            PositionCount = positions.Length,
            Width = width,
            Color = color,
            EffectDataIndex = effectDataIndex,
        });
        graphics.Cache.Add(CommandType.DrawTrail, trailDataIndex);

        return this;
    }

    public readonly Pipeline DrawBasicTrail(
        ReadOnlySpan<Vector2> positions,
        Func<float, float> width,
        Texture2D texture,
        Color color,
        int spriteRotation = 0
    ) {
        var effect = Shaders.Trail.Default.Value;
        ReadOnlySpan<(string, ParameterValue)> parameters = [
            ("sampleTexture", texture),
                ("color", color.ToVector4()),
                ("transformationMatrix", Graphics.WorldTransformMatrix),
                ("spriteRotation", spriteRotation)
        ];

        return DrawTrail(positions, width, static _ => Color.White, effect, parameters);
    }

    public readonly Pipeline DrawBasicTrail(
        ReadOnlySpan<Vector2> positions,
        Func<float, float> width,
        Texture2D texture,
        Func<float, Color> color,
        int spriteRotation = 0
    ) {
        var effect = Shaders.Trail.Default.Value;
        ReadOnlySpan<(string, ParameterValue)> parameters = [
            ("sampleTexture", texture),
                ("color", Color.White.ToVector4()),
                ("transformationMatrix", Graphics.WorldTransformMatrix),
                ("spriteRotation", spriteRotation)
        ];

        return DrawTrail(positions, width, color, effect, parameters);
    }
    
    public readonly Pipeline DrawTexturedIndexedMesh(
        VertexPositionColorTexture[] vertices,
        short[] indices,
        PrimitiveType primitiveType,
        int primitiveCount,
        Effect effect,
        params ReadOnlySpan<(string, ParameterValue)> parameters
    ) {
        var effectDataIndex = AddEffectData(effect, parameters);

        var meshVerticesIndex = graphics.VertexPositionColorTextureDatas.Count;
        graphics.VertexPositionColorTextureDatas.AddRange(vertices);

        var meshIndicesIndex = graphics.IndexDatas.Count;
        graphics.IndexDatas.AddRange(indices);

        var texturedMeshDataIndex = graphics.TexturedIndexedMeshDatas.Count;
        graphics.TexturedIndexedMeshDatas.Add(new() {
            VerticesIndex = meshVerticesIndex,
            VertexCount = vertices.Length,
            IndicesIndex = meshIndicesIndex,
            IndexCount = indices.Length,
            PrimitiveType = primitiveType,
            PrimitiveCount = primitiveCount,
            EffectDataIndex = effectDataIndex,
        });
        graphics.Cache.Add(CommandType.DrawTexturedIndexedMesh, texturedMeshDataIndex);

        return this;
    }

    public readonly Pipeline SetBlendState(BlendState blendState) {
        var index = graphics.BlendStateData.Count;
        graphics.BlendStateData.Add(blendState);

        graphics.Cache.Add(CommandType.SetBlendState, index);
        return this;
    }

    public readonly Pipeline SetSamplerState(int index, SamplerState samplerState) {
        var i = graphics.SamplerStateDatas.Count;
        graphics.SamplerStateDatas.Add(new(index, samplerState));

        graphics.Cache.Add(CommandType.SetSamplerState, i);
        return this;
    }

    public readonly Pipeline SetTexture(int index, Texture2D texture, SamplerState samplerState = null) {
        if(samplerState is { } state) SetSamplerState(index, state);

        var i = graphics.TextureDatas.Count;
        graphics.TextureDatas.Add(new(index, texture));

        graphics.Cache.Add(CommandType.SetTexture, i);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Pipeline EffectParams(
        Effect effect,
        params ReadOnlySpan<(string, ParameterValue)> parameters
    ) {
        graphics.Cache.Add(CommandType.EffectParams, AddEffectData(effect, parameters));
        return this;
    }

    readonly int AddEffectData(Effect effect, ReadOnlySpan<(string, ParameterValue)> parameters) {
        var parameterIndex = graphics.EffectParameterDatas.Count;
        var parameterCount = parameters.Length;
        foreach(var (name, value) in parameters) {
            // This is literally just what 'effect.Parameters[name]' does.
            // And I feel like its better to fail here rather than when actually drawing.
            var i = 0;
            for(; i < effect.Parameters.Count; i++) {
                if(effect.Parameters[i].Name == name) break;
            }

            if(i == effect.Parameters.Count) {
                graphics.Cache.Clear();
                throw new Exception($"Invalid parameter name '{name}'.");
            }

            graphics.EffectParameterDatas.Add(new()
            {
                Index = i,
                Value = value,
            });
        }

        var index = graphics.EffectDatas.Count;
        graphics.EffectDatas.Add(new()
        {
            Effect = effect,
            ParameterIndex = parameterIndex,
            ParameterCount = parameterCount,
        });
        return index;
    }

    public readonly Pipeline DrawSprite(
        Texture2D texture,
        Vector2 position,
        Color? color = null,
        Rectangle? source = null,
        float rotation = 0f,
        Vector2? origin = null,
        Vector2? scale = null,
        SpriteEffects spriteEffects = SpriteEffects.None,
        Effect effect = null
    ) {
        var actualScale = scale ?? Vector2.One;
        var actualSource = source ?? new Rectangle(0, 0, texture.Width, texture.Height);
        return DrawSprite(
            texture,
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)(actualSource.Width * actualScale.X),
                (int)(actualSource.Height * actualScale.Y)
            ),
            color ?? Color.White,
            actualSource,
            rotation,
            origin ?? Vector2.Zero,
            spriteEffects,
            Graphics.ScreenTransformMatrix,
            effect
        );
    }

    public readonly Pipeline DrawSprite(
        Texture2D texture,
        Rectangle destination,
        Color? color = null,
        Rectangle? source = null,
        float rotation = 0f,
        Vector2? origin = null,
        SpriteEffects spriteEffects = SpriteEffects.None,
        Effect effect = null
    ) {
        return DrawSprite(
            texture,
            destination,
            color ?? Color.White,
            source ?? new Rectangle(0, 0, texture.Width, texture.Height),
            rotation,
            origin ?? Vector2.Zero,
            spriteEffects,
            Graphics.ScreenTransformMatrix,
            effect
        );
    }

    readonly Pipeline DrawSprite(
        Texture2D texture,
        Rectangle destination,
        Color color,
        Rectangle source,
        float rotation,
        Vector2 origin,
        SpriteEffects spriteEffects,
        Matrix transformMatrix,
        Effect effect
    ) {
        var sin = MathF.Sin(rotation);
        var cos = MathF.Cos(rotation);

        var size = destination.Size();
        var oX = origin.X * destination.Width / texture.Width;
        var oY = origin.Y * destination.Height / texture.Height;

        var matrix = new Matrix
        {
            M11 = cos * size.X,
            M21 = -sin * size.Y,
            M31 = 0f,
            M41 = destination.X - oX * cos + oY * sin,

            M12 = sin * size.X,
            M22 = cos * size.Y,
            M32 = 0f,
            M42 = destination.Y - oX * sin - oY * cos,

            M13 = 0f,
            M23 = 0f,
            M33 = 1f,
            M43 = 0f,

            M14 = 0f,
            M24 = 0f,
            M34 = 0f,
            M44 = 1f,
        };

        matrix *= transformMatrix;

        var sourceNormalized = new Vector4(
            (float)source.X / texture.Width,
            (float)(source.Y + source.Height) / texture.Height,
            (float)source.Width / texture.Width,
            (float)-source.Height / texture.Height
        );

        ReadOnlySpan<float> offX = [0f, 1f, 0f, 1f];
        ReadOnlySpan<float> offY = [0f, 0f, 1f, 1f];

        var effects = (byte)spriteEffects;
        sourceNormalized.X += sourceNormalized.Z * offX[effects];
        sourceNormalized.Y += sourceNormalized.W * offY[effects];
        sourceNormalized.Z -= 2f * sourceNormalized.Z * offX[effects];
        sourceNormalized.W -= 2f * sourceNormalized.W * offY[effects];

        var spriteDatasIndex = graphics.SpriteDatas.Count;
        graphics.SpriteDatas.Add(new()
        {
            Texture = texture,
            Color = color,
            Source = sourceNormalized,
            Matrix = matrix,
            Effect = effect,
        });
        graphics.Cache.Add(CommandType.DrawSprite, spriteDatasIndex);
        return this;
    }

    public readonly Pipeline ApplyTint(Color color) {
        ApplyEffect(Shaders.Fragment.Tint.Value, ("color", color.ToVector4()));
        return this;
    }

    public readonly Pipeline ApplyEffect(Effect effect, params ReadOnlySpan<(string, ParameterValue)> parameters) {
        var effectDataIndex = AddEffectData(effect, parameters);
        graphics.Cache.Add(CommandType.ApplyEffect, effectDataIndex);
        return this;
    }

    public readonly void Flush() {
        graphics.Cache.Add(CommandType.End, -1);
        graphics.RunCommands(in graphics.Cache);
        graphics.Cache.Clear();
    }

    public readonly void Schedule(RenderLayer layer) {
        graphics.Cache.Add(CommandType.End, -1);
        switch(layer) {
            case RenderLayer.BeforeTiles:
                graphics.BeforeTiles.AddRange(in graphics.Cache);
                break;
            case RenderLayer.AfterTiles:
                graphics.AfterTiles.AddRange(in graphics.Cache);
                break;
            case RenderLayer.BeforeProjectiles:
                graphics.BeforeProjectiles.AddRange(in graphics.Cache);
                break;
            case RenderLayer.AfterProjectiles:
                graphics.AfterProjectiles.AddRange(in graphics.Cache);
                break;
            case RenderLayer.BeforeNPCs:
                graphics.BeforeNPCs.AddRange(in graphics.Cache);
                break;
            case RenderLayer.AfterNPCs:
                graphics.AfterNPCs.AddRange(in graphics.Cache);
                break;
            case RenderLayer.BeforePlayers:
                graphics.BeforePlayers.AddRange(in graphics.Cache);
                break;
            case RenderLayer.AfterPlayers:
                graphics.AfterPlayers.AddRange(in graphics.Cache);
                break;
        }

        graphics.Cache.Clear();
    }
}
