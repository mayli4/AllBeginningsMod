using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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

[Autoload(Side = ModSide.Client)]
public class Graphics : ModSystem {
    private Matrix _worldTransformMatrix;
    public static Matrix WorldTransformMatrix => _instance._worldTransformMatrix;

    private Matrix _screenTransformMatrix;
    public static Matrix ScreenTransformMatrix => _instance._screenTransformMatrix;

    public readonly List<EffectParameterData> EffectParameterDatas = [];
    public readonly List<TextureData> TextureDatas = [];
    public readonly List<DrawSpriteData> SpriteDatas = [];
    public readonly List<SamplerStateData> SamplerStateDatas = [];
    public readonly List<BlendState> BlendStateData = [];
    public readonly List<DrawTrailData> TrailDatas = [];
    public readonly List<BeginData> BeginDatas = [];
    public readonly List<EffectData> EffectDatas = [];
    public readonly List<Vector2> PositionDatas = [];

    public readonly List<VertexPositionColorTexture> VertexPositionColorTextureDatas = [];
    public readonly List<short> IndexDatas = [];
    public readonly List<DrawTexturedIndexedMeshData> TexturedIndexedMeshDatas = [];

    public Commands Cache = new();

    public Commands BeforeTiles = new();
    public Commands AfterTiles = new();
    public Commands BeforeProjectiles = new();
    public Commands AfterProjectiles = new();
    public Commands BeforeNPCs = new();
    public Commands AfterNPCs = new();
    public Commands BeforePlayers = new();
    public Commands AfterPlayers = new();

    const int TrailPositionCapacity = 256;
    const int TrailVertexCount = TrailPositionCapacity * 2;
    const int TrailIndexCount = (TrailPositionCapacity - 1) * 6;

    public DynamicVertexBuffer TrailVertexBuffer;
    public readonly VertexPositionColorTexture[] TrailVertices = new VertexPositionColorTexture[TrailVertexCount];

    public DynamicIndexBuffer TrailIndexBuffer;
    public readonly ushort[] TrailIndices = new ushort[TrailIndexCount];

    public readonly Semaphore TargetSemaphore = new(0, 1);
    public RenderTarget2D ActiveTarget;
    public RenderTarget2D InactiveTarget;

    public Effect SpriteEffect;
    public nint SpriteMatrix;
    public nint SpriteColor;
    public nint SpriteSource;

    public VertexBuffer SpriteVertexBuffer;

    static GraphicsDevice GraphicsDevice => Main.graphics.GraphicsDevice;
    static RenderTarget2D InitFullScreenTarget => new(GraphicsDevice, Main.screenWidth, Main.screenHeight);

    static Graphics _instance;

    public override void Load() {
        _instance = this;

        Threading.RunOnMainThread(() =>
        {
            TrailVertexBuffer = new DynamicVertexBuffer(
                GraphicsDevice,
                typeof(VertexPositionColorTexture),
                TrailPositionCapacity * 2,
                BufferUsage.WriteOnly
            );
            TrailIndexBuffer = new DynamicIndexBuffer(
                GraphicsDevice,
                IndexElementSize.SixteenBits,
                (TrailPositionCapacity - 1) * 6,
                BufferUsage.WriteOnly
            );

            ActiveTarget = InitFullScreenTarget;
            InactiveTarget = InitFullScreenTarget;
            TargetSemaphore.Release();

            SpriteVertexBuffer = new VertexBuffer(
                GraphicsDevice,
                new VertexDeclaration(new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)),
                4,
                BufferUsage.WriteOnly
            );
            SpriteVertexBuffer.SetData([0f, 1f, 2f, 3f]);

            SpriteEffect = Shaders.Trail.Quad.Value;
            SpriteMatrix = SpriteEffect.Parameters["uMatrix"].values;
            SpriteColor = SpriteEffect.Parameters["uColor"].values;
            SpriteSource = SpriteEffect.Parameters["uSource"].values;
        });

        Threading.RunOnMainThread(() =>
        {
            TargetSemaphore.WaitOne();

            ActiveTarget.Dispose();
            InactiveTarget.Dispose();

            ActiveTarget = InitFullScreenTarget;
            InactiveTarget = InitFullScreenTarget;

            TargetSemaphore.Release();
        });

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

        Threading.RunOnMainThread(() =>
        {
            ActiveTarget.Dispose();
            InactiveTarget.Dispose();
            SpriteEffect.Dispose();
        });
    }

    private void On_Main_DrawSuperSpecialProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch) {
        RunCommands(in BeforeProjectiles);
        orig(self, projCache, startSpriteBatch);
    }

    private void On_Main_DrawCachedProjs(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch) {
        orig(self, projCache, startSpriteBatch);
        RunCommands(in AfterProjectiles);
    }

    private void On_Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        RunCommands(in BeforePlayers);
        orig(self);
        RunCommands(in AfterPlayers);
        PostDraw();
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
            PreDraw();
            RunCommands(in BeforeTiles);
            orig(self, behindTiles);
        }
        else {
            RunCommands(in AfterTiles);
            RunCommands(in BeforeNPCs);
            orig(self, behindTiles);
            RunCommands(in AfterNPCs);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PreDraw() {
        _screenTransformMatrix = Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
        _worldTransformMatrix = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
            * ScreenTransformMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PostDraw() {
        EffectParameterDatas.Clear();

        TextureDatas.Clear();
        SpriteDatas.Clear();
        SamplerStateDatas.Clear();
        BlendStateData.Clear();
        TrailDatas.Clear();
        BeginDatas.Clear();
        EffectDatas.Clear();
        PositionDatas.Clear();

        BeforeTiles.Clear();
        AfterTiles.Clear();
        BeforeProjectiles.Clear();
        AfterProjectiles.Clear();
        BeforeNPCs.Clear();
        AfterNPCs.Clear();
        BeforePlayers.Clear();
        AfterPlayers.Clear();
    }

    public static Pipeline BeginPipeline(float scale = 1f) {
        if(_instance.Cache.Count != 0) {
            _instance.Cache.Clear();
            throw new Exception("One pipeline can be begun at a time.");
        }
        var beginDataIndex = _instance.BeginDatas.Count;
        _instance.BeginDatas.Add(new() { Scale = Math.Clamp(scale, 0f, 1f) });

        _instance.Cache.Add(CommandType.Begin, beginDataIndex);
        return new(_instance);
    }

    public void RunCommands(in Commands commands) {
        TargetSemaphore.WaitOne();
        var r = new CommandRunner(this);

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
                case CommandType.DrawTexturedIndexedMesh:
                    r.RunDrawTexturedIndexedMesh(dataIndex);
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
        TargetSemaphore.Release();
    }

    struct CommandRunner(Graphics graphics) {
        float _targetScale;
        RenderTargetBinding[] _cachedBindings;
        RenderTargetUsage _cachedUsage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunSetBlendState(int index) {
            GraphicsDevice.BlendState = graphics.BlendStateData[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunSetTexture(int index) {
            var textureData = graphics.TextureDatas[index];
            GraphicsDevice.Textures[textureData.Index] = textureData.Texture;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunSetSamplerState(int index) {
            var samplerStateData = graphics.SamplerStateDatas[index];
            GraphicsDevice.SamplerStates[samplerStateData.Index] = samplerStateData.State;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunDrawTrail(int index) {
            var trailData = graphics.TrailDatas[index];
            if(trailData.PositionCount < 2) return;

            var trailPositions = CollectionsMarshal
                .AsSpan(graphics.PositionDatas)[trailData.PositionsIndex..(trailData.PositionsIndex + trailData.PositionCount)];

            Color color = trailData.Color(0f);
            Vector2 vertexOffset = trailPositions[0]
                .DirectionTo(trailPositions[1])
                .RotatedBy(MathHelper.PiOver2) * trailData.Width(0f) * 0.5f;

            graphics.TrailVertices[0] = new VertexPositionColorTexture((trailPositions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
            graphics.TrailVertices[1] = new VertexPositionColorTexture((trailPositions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

            for(var j = 1; j < trailPositions.Length; j++) {
                var factor = j / (trailPositions.Length - 1f);

                color = trailData.Color(factor);

                var currentPosition = trailPositions[j];
                var previousPosition = trailPositions[j - 1];

                vertexOffset = previousPosition.DirectionTo(currentPosition).RotatedBy(MathHelper.PiOver2) * trailData.Width(factor) * 0.5f;

                graphics.TrailVertices[j * 2] = new VertexPositionColorTexture(
                    (currentPosition - vertexOffset).ToVector3(),
                    color,
                    new(factor, 0f)
                );
                graphics.TrailVertices[j * 2 + 1] = new VertexPositionColorTexture(
                    (currentPosition + vertexOffset).ToVector3(),
                    color,
                    new(factor, 1f)
                );

                graphics.TrailIndices[(j - 1) * 6] = (ushort)((j - 1) * 2);
                graphics.TrailIndices[(j - 1) * 6 + 1] = (ushort)((j - 1) * 2 + 2);
                graphics.TrailIndices[(j - 1) * 6 + 2] = (ushort)((j - 1) * 2 + 3);
                graphics.TrailIndices[(j - 1) * 6 + 3] = (ushort)((j - 1) * 2 + 3);
                graphics.TrailIndices[(j - 1) * 6 + 4] = (ushort)((j - 1) * 2 + 1);
                graphics.TrailIndices[(j - 1) * 6 + 5] = (ushort)((j - 1) * 2);
            }

            graphics.TrailVertexBuffer.SetData(graphics.TrailVertices);
            GraphicsDevice.SetVertexBuffer(graphics.TrailVertexBuffer);

            graphics.TrailIndexBuffer.SetData(graphics.TrailIndices);
            GraphicsDevice.Indices = graphics.TrailIndexBuffer;

            var effectData = graphics.EffectDatas[trailData.EffectDataIndex];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunDrawTexturedIndexedMesh(int index) {
            var meshData = graphics.TexturedIndexedMeshDatas[index];

            var vertices = CollectionsMarshal.AsSpan(graphics.VertexPositionColorTextureDatas)
                .Slice(meshData.VerticesIndex, meshData.VertexCount);
            var indices = CollectionsMarshal.AsSpan(graphics.IndexDatas)
                .Slice(meshData.IndicesIndex, meshData.IndexCount);

            var effectData = graphics.EffectDatas[meshData.EffectDataIndex];
            SetEffectParams(effectData);

            foreach(var pass in effectData.Effect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(
                    primitiveType: meshData.PrimitiveType,
                    vertexData: vertices.ToArray(),
                    vertexOffset: 0,
                    numVertices: meshData.VertexCount,
                    indexData: indices.ToArray(),
                    indexOffset: 0,
                    primitiveCount: meshData.PrimitiveCount
                );
            }
        }

        readonly void DrawQuad(
            Texture2D texture,
            Matrix matrix,
            Vector4 source,
            Color color,
            Effect effect
        ) {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            unsafe {
                float* ptr = (float*)graphics.SpriteMatrix;
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

                *(Vector4*)graphics.SpriteSource = source;
                *(Vector4*)graphics.SpriteColor = color.ToVector4();
            }

            GraphicsDevice.SetVertexBuffer(graphics.SpriteVertexBuffer);
            GraphicsDevice.Indices = null;

            graphics.SpriteEffect.CurrentTechnique.Passes[0].Apply();
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

        readonly void DrawFullscreenQuad(Texture2D texture, float scale, Effect effect) {
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
        public readonly void RunDrawSprite(int index) {
            var spriteData = graphics.SpriteDatas[index];
            DrawQuad(
                spriteData.Texture,
                spriteData.Matrix,
                spriteData.Source,
                spriteData.Color,
                spriteData.Effect
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RunApplyEffect(int index) {
            var effectData = graphics.EffectDatas[index];

            (graphics.ActiveTarget, graphics.InactiveTarget) = (graphics.InactiveTarget, graphics.ActiveTarget);
            GraphicsDevice.SetRenderTarget(graphics.ActiveTarget);
            GraphicsDevice.Clear(Color.Transparent);

            SetEffectParams(effectData);
            Main.spriteBatch.Begin(new()
            {
                CustomEffect = effectData.Effect,
                TransformMatrix = Matrix.Identity,
            });
            Main.spriteBatch.Draw(graphics.InactiveTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = Main.DefaultSamplerState;

            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunEffectParams(int index) {
            SetEffectParams(graphics.EffectDatas[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RunBegin(int index) {
            var beginData = graphics.BeginDatas[index];
            _targetScale = beginData.Scale;

            _cachedBindings = GraphicsDevice.GetRenderTargets();
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                _cachedUsage = ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage;
                ((RenderTarget2D)_cachedBindings[0].renderTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;
            }

            GraphicsDevice.SetRenderTarget(graphics.ActiveTarget);
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = Main.DefaultSamplerState;

            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void RunEnd(int _) {
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
            Main.spriteBatch.Draw(graphics.ActiveTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), null, Color.White);
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

        readonly void SetEffectParams(EffectData effectData) {
            var effect = effectData.Effect;
            for(var j = 0; j < effectData.ParameterCount; j++) {
                var parameterData = graphics.EffectParameterDatas[j + effectData.ParameterIndex];

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