using AllBeginningsMod.Common.Graphics;
using Daybreak.Common.Rendering;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.GameContent;

namespace AllBeginningsMod.Content.Bosses;

/* todo:
    fix zoom issues
    ball dissipation
 */

/// <summary>
///     Handles all metaball related rendering for the nightgaunt, dreamspawn, etc
/// </summary>
internal sealed class NightgauntMetaballRenderer : ModSystem {
    [StructLayout(LayoutKind.Explicit)]
    private struct Metaball {
        [FieldOffset(0)] public Vector2 Position;
        [FieldOffset(8)] public float Radius;
        [FieldOffset(12)] public Vector2 Velocity;
    }

    private List<Metaball> _metaballs;
    private Vector4[] _metaballData;
    //todo move to multiple buffers for rendering more than 64 bballz
    public const int MAX_METABALLS = 64;

    public override void Load() {
        _metaballs = new List<Metaball>();
        _metaballData = new Vector4[MAX_METABALLS];
    }
    
    public override void PostUpdateEverything() {
        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F) && Main.oldKeyState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F)) {
            if (_metaballs.Count < MAX_METABALLS) {
                _metaballs.Add(new Metaball {
                    Position = Main.MouseWorld, 
                    Radius = 10f, 
                    Velocity = new Vector2(0.01f, 0)
                });
            }
        }
        
        for (int i = 0; i < _metaballs.Count; i++) {
            var ball = _metaballs[i];
            ball.Position += ball.Velocity * Main.GameUpdateCount / 60;
            _metaballs[i] = ball;
        }
    }

    public override void PostDrawTiles() {
        if (_metaballs.Count == 0) return;

        var sb = Main.spriteBatch;
        var effect = Shaders.Nightgaunt.NightgauntMetaball.Value;
        
        sb.Begin(new SpriteBatchSnapshot() { CustomEffect = effect, BlendState = BlendState.AlphaBlend, SamplerState = SamplerState.PointClamp, TransformMatrix = Matrix.Identity});

        for (int i = 0; i < _metaballs.Count; i++) {
            _metaballData[i] = new Vector4(_metaballs[i].Position.X, _metaballs[i].Position.Y, _metaballs[i].Radius, 0.0f);
        }
        
        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        var worldViewDimensions = new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom;
        var topLeftOfScreen = screenCenter - (worldViewDimensions / 2f);

        effect.Parameters["metaballData"].SetValue(_metaballData);
        effect.Parameters["metaballCount"].SetValue(_metaballs.Count);
        effect.Parameters["smoothness"].SetValue(50f); 
        effect.Parameters["screenPos"].SetValue(topLeftOfScreen);
        effect.Parameters["worldViewDimensions"].SetValue(worldViewDimensions);        

        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        sb.End();
    }
}