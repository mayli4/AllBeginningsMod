using System;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace AllBeginningsMod.Utilities;

internal partial class Helper {
    public static T AddElement<T>(this UIElement parent, T child, Action<T> initAction = null) where T : UIElement {
        initAction?.Invoke(child);

        if (parent is UIGrid uiGrid) {
            uiGrid.Add(child);
        }
        else if (parent is UIList uiList) {
            uiList.Add(child);
        }
        else {
            parent.Append(child);
        }

        return child;
    }
    
    public static void SetDimensions
    (
        this UIElement element,
        (float Factor, float Pixels) x = default,
        (float Factor, float Pixels) y = default,
        (float Factor, float Pixels) width = default,
        (float Factor, float Pixels) height = default
    )
    {
        element.Left = new StyleDimension(x.Pixels, x.Factor);
        element.Top = new StyleDimension(y.Pixels, y.Factor);
        element.Width = new StyleDimension(width.Pixels, width.Factor);
        element.Height = new StyleDimension(height.Pixels, height.Factor);
    }
}