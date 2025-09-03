using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common.Graphics;

public enum RenderLayer {
    BeforeTiles,
    AfterTiles,
    BeforeProjectiles,
    AfterProjectiles,
    BeforeNPCs,
    AfterNPCs,
    BeforePlayers,
    AfterPlayers,
}

public class Graphics : ModSystem {
    struct Commands() {
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

    enum CommandType : byte {
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

    record struct SamplerStateData(int Index, SamplerState State);
    record struct TextureData(int Index, Texture2D Texture);

    record struct DrawSpriteData(
        Texture2D Texture,
        Color Color,
        Matrix Matrix,
        Vector4 Source,
        Effect Effect
    );

    record struct DrawTrailData(
        int PositionsIndex,
        int PositionCount,
        Func<float, float> Width,
        Func<float, Color> Color,
        int EffectDataIndex
    );

    record struct BeginData(float Scale);
    record struct EffectData(Effect Effect, int ParameterIndex, int ParameterCount);
    record struct EffectParameterData(int Index, ParameterValue Value);

    [StructLayout(LayoutKind.Explicit)]
    public struct ParameterValue {
        [FieldOffset(0)]
        public ParameterValueType Type;

        // References cannot be overlapped unfortunately..
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

    public static Matrix WorldTransformMatrix { get; private set; }
    public static Matrix ScreenTransformMatrix { get; private set; }

    static readonly List<EffectParameterData> _effectParameters = [];

    static readonly List<TextureData> _textureDatas = [];
    static readonly List<DrawSpriteData> _spriteDatas = [];
    static readonly List<SamplerStateData> _samplerStateDatas = [];
    static readonly List<BlendState> _blendStateData = [];
    static readonly List<DrawTrailData> _trailDatas = [];
    static readonly List<BeginData> _beginDatas = [];
    static readonly List<EffectData> _effectDatas = [];
    static readonly List<Vector2> _positionDatas = [];

    static Commands _cache = new();

    static Commands _beforeTiles = new();
    static Commands _afterTiles = new();
    static Commands _beforeProjectiles = new();
    static Commands _afterProjectiles = new();
    static Commands _beforeNPCs = new();
    static Commands _afterNPCs = new();
    static Commands _beforePlayers = new();
    static Commands _afterPlayers = new();

    const int TrailPositionCapacity = 256;
    const int TrailVertexCount = TrailPositionCapacity * 2;
    const int TrailIndexCount = (TrailPositionCapacity - 1) * 6;

    static DynamicVertexBuffer _trailVertexBuffer;
    static readonly VertexPositionColorTexture[] _trailVertices = new VertexPositionColorTexture[TrailVertexCount];

    static DynamicIndexBuffer _trailIndexBuffer;
    static readonly ushort[] _trailIndices = new ushort[TrailIndexCount];

    readonly static Semaphore _targetSemaphore = new(0, 1);
    static RenderTarget2D _activeTarget;
    static RenderTarget2D _inactiveTarget;

    static Effect _spriteEffect;
    static nint _spriteMatrix;
    static nint _spriteColor;
    static nint _spriteSource;

    static VertexBuffer _spriteVertexBuffer;

    static GraphicsDevice GraphicsDevice => Main.graphics.GraphicsDevice;
    static RenderTarget2D InitFullScreenTarget => new(GraphicsDevice, Main.screenWidth, Main.screenHeight);

    public override void Load() {
        Main.QueueMainThreadAction(() =>
        {
            _trailVertexBuffer = new DynamicVertexBuffer(
                GraphicsDevice,
                typeof(VertexPositionColorTexture),
                TrailPositionCapacity * 2,
                BufferUsage.WriteOnly
            );
            _trailIndexBuffer = new DynamicIndexBuffer(
                GraphicsDevice,
                IndexElementSize.SixteenBits,
                (TrailPositionCapacity - 1) * 6,
                BufferUsage.WriteOnly
            );

            _activeTarget = InitFullScreenTarget;
            _inactiveTarget = InitFullScreenTarget;
            _targetSemaphore.Release();

            _spriteVertexBuffer = new VertexBuffer(
                GraphicsDevice,
                new VertexDeclaration(new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)),
                4,
                BufferUsage.WriteOnly
            );
            _spriteVertexBuffer.SetData([0f, 1f, 2f, 3f]);

            _spriteEffect = Shaders.Trail.Quad.Value;
            _spriteMatrix = _spriteEffect.Parameters["uMatrix"].values;
            _spriteColor = _spriteEffect.Parameters["uColor"].values;
            _spriteSource = _spriteEffect.Parameters["uSource"].values;
        });

        Main.OnResolutionChanged += (screenSize) =>
        {
            Main.QueueMainThreadAction(() =>
            {
                _targetSemaphore.WaitOne();

                _activeTarget.Dispose();
                _inactiveTarget.Dispose();

                _activeTarget = InitFullScreenTarget;
                _inactiveTarget = InitFullScreenTarget;

                _targetSemaphore.Release();
            });
        };

        On_Main.DrawNPCs += On_Main_DrawNPCs;
        On_Main.DrawSuperSpecialProjectiles += On_Main_DrawSuperSpecialProjectiles;
        On_Main.DrawPlayers_AfterProjectiles += On_Main_DrawPlayers_AfterProjectiles;
        On_Main.DrawCachedProjs += On_Main_DrawCachedProjs;
    }

    public override void Unload() {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
        On_Main.DrawSuperSpecialProjectiles -= On_Main_DrawSuperSpecialProjectiles;
        On_Main.DrawPlayers_AfterProjectiles -= On_Main_DrawPlayers_AfterProjectiles;
        On_Main.DrawCachedProjs -= On_Main_DrawCachedProjs;

        Main.QueueMainThreadAction(() =>
        {
            _activeTarget.Dispose();
            _inactiveTarget.Dispose();
            _spriteEffect.Dispose();
        });
    }

    private void On_Main_DrawSuperSpecialProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch) {
        CommandRunner.Run(in _beforeProjectiles);
        orig(self, projCache, startSpriteBatch);
    }

    private void On_Main_DrawCachedProjs(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch) {
        orig(self, projCache, startSpriteBatch);
        CommandRunner.Run(in _afterProjectiles);
    }

    private void On_Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        CommandRunner.Run(in _beforePlayers);
        orig(self);
        CommandRunner.Run(in _afterPlayers);
        PostDraw();
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
            PreDraw();
            CommandRunner.Run(in _beforeTiles);
            orig(self, behindTiles);
        }
        else {
            CommandRunner.Run(in _afterTiles);
            CommandRunner.Run(in _beforeNPCs);
            orig(self, behindTiles);
            CommandRunner.Run(in _afterNPCs);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void PreDraw() {
        ScreenTransformMatrix = Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
        WorldTransformMatrix = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
            * ScreenTransformMatrix;
    }

    static void PostDraw() {
        _effectParameters.Clear();

        _textureDatas.Clear();
        _spriteDatas.Clear();
        _samplerStateDatas.Clear();
        _blendStateData.Clear();
        _trailDatas.Clear();
        _beginDatas.Clear();
        _effectDatas.Clear();
        _positionDatas.Clear();

        _beforeTiles.Clear();
        _afterTiles.Clear();
        _beforeProjectiles.Clear();
        _afterProjectiles.Clear();
        _beforeNPCs.Clear();
        _afterNPCs.Clear();
        _beforePlayers.Clear();
        _afterPlayers.Clear();
    }

    public static Pipeline BeginPipeline(float scale = 1f) {
        if(_cache.Count != 0) throw new Exception("One pipeline can be begun at a time.");

        var beginDataIndex = _beginDatas.Count;
        _beginDatas.Add(new() { Scale = Math.Clamp(scale, 0f, 1f) });

        _cache.Add(CommandType.Begin, beginDataIndex);
        return new();
    }

    // TODO: Place project specific methods in an extension class (ApplyOutline, DrawBasicTrail, etc.).
    public struct Pipeline {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline ApplyOutline(Color color, float threshold = 0.001f) {
            ApplyEffect(
                Shaders.Outline.Value,
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

            var trailPositionsIndex = _positionDatas.Count;
            _positionDatas.AddRange(positions);

            var trailDataIndex = _trailDatas.Count;
            _trailDatas.Add(new()
            {
                PositionsIndex = trailPositionsIndex,
                PositionCount = positions.Length,
                Width = width,
                Color = color,
                EffectDataIndex = effectDataIndex,
            });
            _cache.Add(CommandType.DrawTrail, trailDataIndex);

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
                ("transformationMatrix", WorldTransformMatrix),
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
                ("transformationMatrix", WorldTransformMatrix),
                ("spriteRotation", spriteRotation)
            ];

            return DrawTrail(positions, width, color, effect, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline SetBlendState(BlendState blendState) {
            var index = _blendStateData.Count;
            _blendStateData.Add(blendState);

            _cache.Add(CommandType.SetBlendState, index);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline SetSamplerState(int index, SamplerState samplerState) {
            var i = _samplerStateDatas.Count;
            _samplerStateDatas.Add(new(index, samplerState));

            _cache.Add(CommandType.SetSamplerState, i);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline SetTexture(int index, Texture2D texture, SamplerState samplerState = null) {
            if(samplerState is { } state) SetSamplerState(index, state);

            var i = _textureDatas.Count;
            _textureDatas.Add(new(index, texture));

            _cache.Add(CommandType.SetTexture, i);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline EffectParams(
            Effect effect,
            params ReadOnlySpan<(string, ParameterValue)> parameters
        ) {
            _cache.Add(CommandType.EffectParams, AddEffectData(effect, parameters));
            return this;
        }

        static int AddEffectData(Effect effect, ReadOnlySpan<(string, ParameterValue)> parameters) {
            var parameterIndex = _effectParameters.Count;
            var parameterCount = parameters.Length;
            foreach(var (name, value) in parameters) {
                // This is literally just what 'effect.Parameters[name]' does.
                // And I feel like its better to fail here rather than when actually drawing.
                var i = 0;
                for(; i < effect.Parameters.Count; i++) {
                    if(effect.Parameters[i].Name == name) break;
                }

                if(i == effect.Parameters.Count) {
                    _cache.Clear();
                    throw new Exception($"Invalid parameter name '{name}'.");
                }

                _effectParameters.Add(new()
                {
                    Index = i,
                    Value = value,
                });
            }

            var index = _effectDatas.Count;
            _effectDatas.Add(new()
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
                ScreenTransformMatrix,
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
                ScreenTransformMatrix,
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

            var spriteDatasIndex = _spriteDatas.Count;
            _spriteDatas.Add(new()
            {
                Texture = texture,
                Color = color,
                Source = sourceNormalized,
                Matrix = matrix,
                Effect = effect,
            });
            _cache.Add(CommandType.DrawSprite, spriteDatasIndex);
            return this;
        }

        public readonly Pipeline ApplyTint(Color color) {
            ApplyEffect(Shaders.Tint.Value, ("color", color.ToVector4()));
            return this;
        }

        public readonly Pipeline ApplyEffect(Effect effect, params ReadOnlySpan<(string, ParameterValue)> parameters) {
            var effectDataIndex = AddEffectData(effect, parameters);
            _cache.Add(CommandType.ApplyEffect, effectDataIndex);
            return this;
        }

        public readonly void Flush() {
            _cache.Add(CommandType.End, -1);
            CommandRunner.Run(in _cache);
            _cache.Clear();
        }

        public readonly void Schedule(RenderLayer layer) {
            _cache.Add(CommandType.End, -1);
            switch(layer) {
                case RenderLayer.BeforeTiles:
                    _beforeTiles.AddRange(in _cache);
                    break;
                case RenderLayer.AfterTiles:
                    _afterTiles.AddRange(in _cache);
                    break;
                case RenderLayer.BeforeProjectiles:
                    _beforeProjectiles.AddRange(in _cache);
                    break;
                case RenderLayer.AfterProjectiles:
                    _afterProjectiles.AddRange(in _cache);
                    break;
                case RenderLayer.BeforeNPCs:
                    _beforeNPCs.AddRange(in _cache);
                    break;
                case RenderLayer.AfterNPCs:
                    _afterNPCs.AddRange(in _cache);
                    break;
                case RenderLayer.BeforePlayers:
                    _beforePlayers.AddRange(in _cache);
                    break;
                case RenderLayer.AfterPlayers:
                    _afterPlayers.AddRange(in _cache);
                    break;
            }

            _cache.Clear();
        }
    }

    struct CommandRunner {
        float _targetScale;

        RenderTargetBinding[] _cachedBindings;
        RenderTargetUsage _cachedUsage;

        public static void Run(in Commands commands) {
            _targetSemaphore.WaitOne();
            var r = new CommandRunner();

            SpriteBatchSnapshot? snapshot = null;
            if(Main.spriteBatch.beginCalled) {
                Main.spriteBatch.End(out var s);
                snapshot = s;
            }

            for(var i = 0; i < commands.Count; i++) {
                var dataIndex = commands.Datas[i];
                switch(commands.Types[i]) {
                    case CommandType.DrawTrail:
                        r.RunDrawTrail(dataIndex);
                        break;
                    case CommandType.DrawSprite:
                        r.RunDrawSprite(dataIndex);
                        break;
                    case CommandType.Begin:
                        r.RunBegin(dataIndex);
                        break;
                    case CommandType.End:
                        r.RunEnd(dataIndex);
                        break;
                    case CommandType.ApplyEffect:
                        r.RunApplyEffect(dataIndex);
                        break;
                    case CommandType.EffectParams:
                        r.RunEffectParams(dataIndex);
                        break;
                    case CommandType.SetBlendState:
                        r.RunSetBlendState(dataIndex);
                        break;
                    case CommandType.SetTexture:
                        r.RunSetTexture(dataIndex);
                        break;
                    case CommandType.SetSamplerState:
                        r.RunSetSamplerState(dataIndex);
                        break;
                }
            }

            // This fixes the issue with vanilla trail being drawn 2x bigger in case of half size target..
            // The spritebatch sets the transformation matrix in `End`
            // and the trails depend on it so it needs to be set back to normal.
            Main.spriteBatch.Begin(new());
            Main.spriteBatch.End();

            if(snapshot != null) Main.spriteBatch.Begin(snapshot.Value);
            _targetSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunSetBlendState(int index) {
            GraphicsDevice.BlendState = _blendStateData[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunSetTexture(int index) {
            var textureData = _textureDatas[index];
            GraphicsDevice.Textures[textureData.Index] = textureData.Texture;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunSetSamplerState(int index) {
            var samplerStateData = _samplerStateDatas[index];
            GraphicsDevice.SamplerStates[samplerStateData.Index] = samplerStateData.State;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunDrawTrail(int index) {
            var trailData = _trailDatas[index];
            if(trailData.PositionCount < 2) return;

            var trailPositions = CollectionsMarshal
                .AsSpan(_positionDatas)[trailData.PositionsIndex..(trailData.PositionsIndex + trailData.PositionCount)];

            Color color = trailData.Color(0f);
            Vector2 vertexOffset = trailPositions[0]
                .DirectionTo(trailPositions[1])
                .RotatedBy(MathHelper.PiOver2) * trailData.Width(0f) * 0.5f;

            _trailVertices[0] = new VertexPositionColorTexture((trailPositions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
            _trailVertices[1] = new VertexPositionColorTexture((trailPositions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

            for(var j = 1; j < trailPositions.Length; j++) {
                var factor = j / (trailPositions.Length - 1f);

                color = trailData.Color(factor);

                var currentPosition = trailPositions[j];
                var previousPosition = trailPositions[j - 1];

                vertexOffset = previousPosition.DirectionTo(currentPosition).RotatedBy(MathHelper.PiOver2) * trailData.Width(factor) * 0.5f;

                _trailVertices[j * 2] = new VertexPositionColorTexture(
                    (currentPosition - vertexOffset).ToVector3(),
                    color,
                    new(factor, 0f)
                );
                _trailVertices[j * 2 + 1] = new VertexPositionColorTexture(
                    (currentPosition + vertexOffset).ToVector3(),
                    color,
                    new(factor, 1f)
                );

                _trailIndices[(j - 1) * 6] = (ushort)((j - 1) * 2);
                _trailIndices[(j - 1) * 6 + 1] = (ushort)((j - 1) * 2 + 2);
                _trailIndices[(j - 1) * 6 + 2] = (ushort)((j - 1) * 2 + 3);
                _trailIndices[(j - 1) * 6 + 3] = (ushort)((j - 1) * 2 + 3);
                _trailIndices[(j - 1) * 6 + 4] = (ushort)((j - 1) * 2 + 1);
                _trailIndices[(j - 1) * 6 + 5] = (ushort)((j - 1) * 2);
            }

            _trailVertexBuffer.SetData(_trailVertices);
            GraphicsDevice.SetVertexBuffer(_trailVertexBuffer);

            _trailIndexBuffer.SetData(_trailIndices);
            GraphicsDevice.Indices = _trailIndexBuffer;

            var effectData = _effectDatas[trailData.EffectDataIndex];
            SetEffectParams(effectData);

            foreach(EffectPass pass in effectData.Effect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    trailPositions.Length * 2,
                    0,
                    (trailPositions.Length - 1) * 2
                );
            }
        }

        static void DrawQuad(
            Texture2D texture,
            Matrix matrix,
            Vector4 source,
            Color color,
            Effect effect
        ) {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = Main.Rasterizer;

            unsafe {
                float* ptr = (float*)_spriteMatrix;
                *ptr = matrix.M11;
                ptr[1] = matrix.M21;
                ptr[2] = matrix.M31;
                ptr[3] = matrix.M41;
                ptr[4] = matrix.M12;
                ptr[5] = matrix.M22;
                ptr[6] = matrix.M32;
                ptr[7] = matrix.M42;
                ptr[8] = matrix.M13;
                ptr[9] = matrix.M23;
                ptr[10] = matrix.M33;
                ptr[11] = matrix.M43;
                ptr[12] = matrix.M14;
                ptr[13] = matrix.M24;
                ptr[14] = matrix.M34;
                ptr[15] = matrix.M44;

                *(Vector4*)_spriteSource = source;
                *(Vector4*)_spriteColor = color.ToVector4();
            }

            GraphicsDevice.SetVertexBuffer(_spriteVertexBuffer);
            GraphicsDevice.Indices = null;

            _spriteEffect.CurrentTechnique.Passes[0].Apply();
            if(effect is { } e) {
                foreach(var pass in e.CurrentTechnique.Passes) {
                    pass.Apply();
                    GraphicsDevice.Textures[0] = texture;
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                }

                return;
            }

            GraphicsDevice.Textures[0] = texture;
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        static void DrawFullscreenQuad(Texture2D texture, float scale, Effect effect) {
            DrawQuad(
                texture,
                new Matrix(
                    2f * scale, 0f, 0f, 0f,
                    0f, 2f * scale, 0f, 0f,
                    0f, 0f, 1f, 0f,
                    -1f * scale, -1f * scale, 0f, 1f
                ),
                new(0, 0, 1, 1),
                Color.White,
                effect
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunDrawSprite(int index) {
            var spriteData = _spriteDatas[index];
            DrawQuad(
                spriteData.Texture,
                spriteData.Matrix,
                spriteData.Source,
                spriteData.Color,
                spriteData.Effect
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunApplyEffect(int index) {
            var effectData = _effectDatas[index];

            (_activeTarget, _inactiveTarget) = (_inactiveTarget, _activeTarget);
            GraphicsDevice.SetRenderTarget(_activeTarget);
            GraphicsDevice.Clear(Color.Transparent);

            SetEffectParams(effectData);
            Main.spriteBatch.Begin(new()
            {
                CustomEffect = effectData.Effect,
                TransformMatrix = Matrix.Identity,
            });
            Main.spriteBatch.Draw(_inactiveTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = Main.DefaultSamplerState;

            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunEffectParams(int index) {
            SetEffectParams(_effectDatas[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunBegin(int index) {
            var beginData = _beginDatas[index];
            _targetScale = beginData.Scale;

            _cachedBindings = GraphicsDevice.GetRenderTargets();
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                _cachedUsage = ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage;
                ((RenderTarget2D)_cachedBindings[0].renderTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;
            }

            GraphicsDevice.SetRenderTarget(_activeTarget);
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = Main.DefaultSamplerState;

            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunEnd(int _) {
            GraphicsDevice.SetRenderTargets(_cachedBindings);
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage = _cachedUsage;
            }

            Main.spriteBatch.Begin(new()
            {
                TransformMatrix = Matrix.CreateScale(
                    Main.GameViewMatrix.Zoom.X / _targetScale,
                    Main.GameViewMatrix.Zoom.Y / _targetScale,
                    0f
                ),
            });
            Main.spriteBatch.Draw(_activeTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), null, Color.White);
            Main.spriteBatch.End();

            // DrawFullscreenQuad(_activeTarget, 1f / _targetScale, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void SetTargetViewport() {
            GraphicsDevice.Viewport = new(
                0,
                0,
                (int)(Main.screenWidth * _targetScale / Main.GameViewMatrix.Zoom.X),
                (int)(Main.screenHeight * _targetScale / Main.GameViewMatrix.Zoom.Y)
            );
        }

        static void SetEffectParams(EffectData effectData) {
            var effect = effectData.Effect;
            for(var j = 0; j < effectData.ParameterCount; j++) {
                var parameterData = _effectParameters[j + effectData.ParameterIndex];

                var parameter = effect.Parameters.elements[parameterData.Index];
                switch(parameterData.Value.Type) {
                    case ParameterValueType.Int:
                        parameter.SetValue(parameterData.Value.Int);
                        break;
                    case ParameterValueType.Float:
                        parameter.SetValue(parameterData.Value.Float);
                        break;
                    case ParameterValueType.Vector2:
                        parameter.SetValue(parameterData.Value.Vector2);
                        break;
                    case ParameterValueType.Vector3:
                        parameter.SetValue(parameterData.Value.Vector3);
                        break;
                    case ParameterValueType.Vector4:
                        parameter.SetValue(parameterData.Value.Vector4);
                        break;
                    case ParameterValueType.Texture2D:
                        parameter.SetValue(parameterData.Value.Texture2D);
                        break;
                    case ParameterValueType.Matrix:
                        parameter.SetValue(parameterData.Value.Matrix);
                        break;
                }
            }
        }
    }
}
