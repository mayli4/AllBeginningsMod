using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.ModPanel;
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

    [UsedImplicitly]
    [OnLoad(Side = ModSide.Client)]
    static void KillBaubles() {
        MonoModHooks.Modify(getMethod(nameof(UIModItem.OnInitialize)), il =>
        {
            var c = new ILCursor(il);
        });
        
        return;
        
        static MethodInfo getMethod(string name) => typeof(UIModItem).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
    }

    public override bool PreDrawPanel(UIModItem element, SpriteBatch sb, ref bool drawDivider) {
        if (element._needsTextureLoading)
        {
            element._needsTextureLoading = false;
            element.LoadTextures();
        }

        GraphicsDevice device = Main.instance.GraphicsDevice;

        var dims = element.GetDimensions();

        // Make sure the panel draws correctly on any scale.
        Vector2 size = Vector2.Transform(dims.ToRectangle().Size(), Main.UIScaleMatrix);

        return true;
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
}