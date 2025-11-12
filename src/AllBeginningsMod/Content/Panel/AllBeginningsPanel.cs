using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.ModPanel;
using Daybreak.Common.Rendering;
using JetBrains.Annotations;
using MonoMod.Cil;
using ReLogic.Content;
using System.Reflection;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace AllBeginningsMod.Content.Panel;

[UsedImplicitly]
internal sealed class AllBeginningsPanel : ModPanelStyleExt {
    public override UIImage ModifyModIcon(UIModItem element, UIImage modIcon, ref int modIconAdjust) {
        return new Icon() {
            Left = modIcon.Left,
            Top = modIcon.Top,
            Width = modIcon.Width,
            Height = modIcon.Height,
        };
    }

    public override bool PreDrawPanel(UIModItem element, SpriteBatch spriteBatch, ref bool drawDivider) {
        if (element._needsTextureLoading) {
            element._needsTextureLoading = false;
            element.LoadTextures();
        }

        var device = Main.instance.GraphicsDevice;

        var dims = element.GetDimensions();

        Vector2 size = Vector2.Transform(dims.ToRectangle().Size(), Main.UIScaleMatrix);
        Vector2 position = Vector2.Transform(dims.Position(), Main.UIScaleMatrix);

        Rectangle source = new((int)position.X, (int)position.Y, 
            (int)size.X, (int)size.Y);

        spriteBatch.End(out var snapshot);

        var leasedTarget = RenderTargetPool.Shared.Rent(device, (int)size.X, (int)size.Y, RenderTargetDescriptor.Default);

        using (var _ = new RenderTargetScope(device, leasedTarget.Target, true, true, Color.Transparent))
            DrawPanelBackground(spriteBatch, size, source);

        DrawAsPanel(spriteBatch, snapshot, device, leasedTarget.Target, source, element);
        
        leasedTarget.Dispose();
        
        spriteBatch.Begin(snapshot);
        
        element.DrawPanel(spriteBatch, element._borderTexture.Value, Color.Black);
        drawDivider = false;
        
        spriteBatch.Restart(in snapshot);
        
        return false;
    }
    
    private static void DrawPanelBackground(SpriteBatch spriteBatch, Vector2 size, Rectangle frame)
    {
        var shader = Assets.Shaders.Panel.PanelBackgroundShader.CreatePanelShader();
        shader.Parameters.uSource = new Vector4(frame.Width, frame.Height, frame.X, frame.Y);
        shader.Apply();
    
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, shader.Shader, Matrix.Identity);
        Rectangle background = new(0, 0, (int)size.X, (int)size.Y);
        spriteBatch.Draw(Assets.Textures.Sample.Blobs.Asset.Value, background, Color.White);
        spriteBatch.End(out var snapshot);
    }
    
    private static void DrawAsPanel(SpriteBatch spriteBatch, SpriteBatchSnapshot snapshot, GraphicsDevice device, Texture2D texture, Rectangle frame, UIPanel element, Color? color = null)
    {
        var shader = Assets.Shaders.Panel.PanelProjection.CreatePass1();
        shader.Parameters.Source = new(frame.Width, frame.Height, frame.X, frame.Y);
        shader.Apply();
        
        spriteBatch.Begin(snapshot with { SortMode = SpriteSortMode.Immediate, CustomEffect = shader.Shader});

        device.Textures[1] = texture;
        device.SamplerStates[1] = SamplerState.PointClamp;

        element.DrawPanel(spriteBatch, element._backgroundTexture.Value, color ?? element.BackgroundColor);
        element.DrawPanel(spriteBatch, element._borderTexture.Value, color ?? element.BorderColor);

        spriteBatch.End();
    }
    
    private static Vector4 Transform(Vector4 vector) {
        var vec1 = Vector2.Transform(new Vector2(vector.X, vector.Y), Main.UIScaleMatrix);
        var vec2 = Vector2.Transform(new Vector2(vector.Z, vector.W), Main.UIScaleMatrix);
        return new Vector4(vec1, vec2.X, vec2.Y);
    }

    internal sealed class Icon : UIImage {
        private readonly Asset<Texture2D> iconAsset;
        
        public Icon() : base(TextureAssets.MagicPixel) {
            iconAsset = Assets.Textures.UI.Panel.Gardener.Asset;
            
            Width.Set(96, 0f);
            Height.Set(80, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            int currentFrame = (int)(Main.GlobalTimeWrappedHourly / 0.25f % 3);

            var dims = GetDimensions();
            var origin = new Vector2(-96, 80 / 2);

            var offsetpos = new Vector2(-98, 28); 
            
            spriteBatch.Draw(iconAsset.Value, dims.Position() + offsetpos, null, Color.White, 0, origin, 0.9f, SpriteEffects.None, 0f);
        }
    }
    
    [UsedImplicitly]
    [OnLoad(Side = ModSide.Client)]
    /* todo: remove when pr gets merged or make it apply specifically to allbeginnings */
    static void KillBaubles() {
        MonoModHooks.Modify(getMethod(nameof(UIModItem.OnInitialize)), il => {
            var c = new ILCursor(il);

            // find "loaded = true" instr, go to next instr, skip forward to the end of the `if (loadedMod != null)` block
            if (!c.TryGotoNext(MoveType.After, i => i.MatchStfld<UIModItem>(nameof(UIModItem._loaded)))) {
                return;
            }
            
            // now right before "loaded = true", jump to the instruction right after the for loop
            if (!c.TryGotoNext(MoveType.Before,
                    i => i.MatchLdarg0(),
                    i => i.MatchLdfld<UIModItem>(nameof(UIModItem._loaded)),
                    i => i.MatchBrtrue(out _)
                )) {
                return;
            }

            // mark current cursor pos as a label to jump to
            var targetLabel = c.DefineLabel();
            c.MarkLabel(targetLabel);

            if (!c.TryGotoPrev(MoveType.After, i => i.MatchStfld<UIModItem>(nameof(UIModItem._loaded)))) {
                return;
            }

            // murder
            c.EmitBr(targetLabel);
        });

        return;

        static MethodInfo getMethod(string name) => typeof(UIModItem).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
    }
}