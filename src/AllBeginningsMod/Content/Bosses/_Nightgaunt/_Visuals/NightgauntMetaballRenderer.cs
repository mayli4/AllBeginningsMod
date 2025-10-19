using AllBeginningsMod.Common.Graphics;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AllBeginningsMod.Content.Bosses;

/* todo
 
 - make it support more than 64 potentially, unsure if this is needed since lifetimes handle ball murder most of the time and we wont have a ton onscreen, but might be good to have anyways
 - multiple buffers? ping-pong between buffer a to buffer b and accumulate their combined metaballs
 
 - fix More zooming issues
 
 */

/// <summary>
///     Handles all metaball related rendering for the nightgaunt, dreamspawn, etc
/// </summary>
internal unsafe sealed class NightgauntMetaballRenderer : ModSystem {
    [StructLayout(LayoutKind.Explicit)]
    private struct Metaball {
        [FieldOffset(0)] public Vector2 Position;
        [FieldOffset(8)] public float Radius;
        [FieldOffset(12)] public Vector2 Velocity;
        [FieldOffset(20)] public float TimeLeft;
        [FieldOffset(24)] public float MaxTime;
        [FieldOffset(28)] public float InitialRadius;
    }

    private Metaball[] _metaballs;
    private int _activeMetaballCount;

    private Vector4[] _metaballData;
    private const int max_metaballs = 64;

    private RenderTarget2D _screenBuffer;
    private const float RenderScale = 0.5f;

    public override void Load() {
        _metaballs = new Metaball[max_metaballs];
        _metaballData = new Vector4[max_metaballs];
        
        _activeMetaballCount = 0;
        
        Threading.RunOnMainThread(() =>
        {
            _screenBuffer = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

            Main.OnResolutionChanged += ReinitTargets;
        });
    }

    public override void Unload() {
        Main.OnResolutionChanged -= ReinitTargets;
    }

    void ReinitTargets(Vector2 size) {
        _screenBuffer = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X, (int)size.Y);
    }
    
    public override void PostUpdateEverything() {
        if (!Main.hasFocus) return;

        float dt = 1f / 60f;

        if (Main.keyState.IsKeyDown(Keys.F) && Main.oldKeyState.IsKeyUp(Keys.F)) {
            if (_activeMetaballCount < max_metaballs) {
                float initialRadius = 50f + (float)Main.rand.NextDouble() * 50f;
                float maxTime = 3f + (float)Main.rand.NextDouble() * 2f;

                ref Metaball newBall = ref _metaballs[_activeMetaballCount];
                newBall.Position = Main.MouseWorld;
                newBall.Radius = initialRadius;
                newBall.Velocity = Vector2.Zero;
                newBall.InitialRadius = initialRadius;
                newBall.MaxTime = maxTime;
                newBall.TimeLeft = maxTime;

                _activeMetaballCount++;
            }
        }

        var balls = _metaballs.AsSpan(0, _activeMetaballCount);

        for (int i = balls.Length - 1; i >= 0; i--) {
            ref Metaball ball = ref balls[i];

            ball.TimeLeft -= dt;

            if (ball.TimeLeft <= 0) {
                ball = balls[_activeMetaballCount - 1];
                _activeMetaballCount--;
                continue;
            }

            ball.Position += ball.Velocity * dt;
            float lifeProgress = ball.TimeLeft / ball.MaxTime;
            ball.Radius = ball.InitialRadius * lifeProgress;
        }
    }

    public static void New(Vector2 pos, float radius, float lifetime, Vector2 velocity = default) {
        
    }
    
    public override void PostDrawTiles() {
        if (_activeMetaballCount == 0) return;

        var sb = Main.spriteBatch;
        var gd = Main.instance.GraphicsDevice;
        var effect = Shaders.Nightgaunt.NightgauntMetaball.Value;
        RtContentPreserver.ApplyToBindings(gd.GetRenderTargets());
        var rts = gd.GetRenderTargets();
        RtContentPreserver.ApplyToBindings(rts);
        
        gd.SetRenderTarget(_screenBuffer);
        gd.Clear(Color.Transparent);
        
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);

        var src = _metaballs.AsSpan(0, _activeMetaballCount);
        var dst = _metaballData.AsSpan(0, _activeMetaballCount);

        fixed (Metaball* sourcePtr = src)
        fixed (Vector4* destPtr = dst) {
            for (int i = 0; i < _activeMetaballCount; i++) {
                Metaball* sourceBall = sourcePtr + i;
                Vector4* destVec = destPtr + i;

                destVec->X = sourceBall->Position.X;
                destVec->Y = sourceBall->Position.Y;
                destVec->Z = sourceBall->Radius;
                destVec->W = 0.0f;
            }
        }
        
        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom;
        var correctScreenTopLeft = screenCenter - (worldViewDimensions / 2f);
        
        effect.Parameters["metaballData"].SetValue(_metaballData);
        effect.Parameters["metaballCount"].SetValue(_activeMetaballCount);
        effect.Parameters["smoothness"].SetValue(50f); 
        effect.Parameters["screenPos"].SetValue(correctScreenTopLeft);
        effect.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);

        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        sb.End();
        
        gd.SetRenderTargets(rts);
        
        Graphics.BeginPipeline(0.5f).DrawSprite(_screenBuffer, Vector2.Zero, Color.Red).Flush();
    }
}