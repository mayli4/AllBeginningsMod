using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace AllBeginningsMod.Common.Bestiary;

public class ModBiomeSpawnCondition : IFilterInfoProvider, IProvideSearchFilterString, IBestiaryInfoElement, IBestiaryBackgroundImagePathAndColorProvider, IBestiaryPrioritizedElement {
    private readonly string _name;

    private readonly string _iconPath;

    private readonly string _backgroundPath;

    private Color? _color;

    public int DisplayTextPriority { get; set; }

    public bool HideInPortraitInfo { get; set; }

    public float OrderPriority { get; set; }

    public ModBiomeSpawnCondition(string name, string iconPath, string backgroundPath, Color? color = null) {
        _name = name;
        _iconPath = iconPath;
        _backgroundPath = backgroundPath;
        _color = color;
    }

    public Color? GetBackgroundColor() {
        return _color;
    }

    public Asset<Texture2D> GetBackgroundImage() {
        return _backgroundPath == null ? null : ModContent.Request<Texture2D>(_backgroundPath);
    }

    public UIElement GetFilterImage() {
        Asset<Texture2D> asset = ModContent.Request<Texture2D>(_iconPath);
        return new UIImageFramed(asset, new Rectangle(0, 0, asset.Width(), asset.Height())) {
            HAlign = 0.5f,
            VAlign = 0.5f
        };
    }

    public string GetDisplayNameKey() {
        return _name;
    }

    public string GetSearchString(ref BestiaryUICollectionInfo info) {
        return info.UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0 ? null : _name;
    }

    public UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
        if (HideInPortraitInfo)
            return null;

        UIElement uIElement = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7) {
            Width = new StyleDimension(-14f, 1f),
            Height = new StyleDimension(34f, 0f),
            BackgroundColor = new Color(43, 56, 101),
            BorderColor = Color.Transparent,
            Left = new StyleDimension(5f, 0f)
        };

        uIElement.SetPadding(0f);
        uIElement.PaddingRight = 5f;

        UIElement filterImage = GetFilterImage();
        filterImage.HAlign = 0f;
        filterImage.Left = new StyleDimension(5f, 0f);

        var element = new UIText(GetDisplayNameKey(), 0.8f) {
            HAlign = 0f,
            Left = new StyleDimension(38f, 0f),
            TextOriginX = 0f,
            TextOriginY = 0f,
            VAlign = 0.5f,
            DynamicallyScaleDownToWidth = true
        };

        if (filterImage != null)
            uIElement.Append(filterImage);

        uIElement.Append(element);
        uIElement.OnUpdate += delegate (UIElement e) {
            if (e.IsMouseHovering) {
                string textValue = GetDisplayNameKey();
                Main.instance.MouseText(textValue, 0, 0);
            }
        };

        return uIElement;
    }
}