using System;
using System.Runtime.CompilerServices;
using Terraria.UI;

namespace AllBeginningsMod.Common.UI;

// github.com/Mirsario/TerrariaOverhaul/blob/dev/Core/Interface/UIComponent.cs

internal abstract class UIComponent {
    private WeakReference<UIElement> _parent;

    public UIElement Parent {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            _parent.TryGetTarget(out var element);
            return element;
        }
    }
    
    protected abstract void OnAttach(UIElement element);

    protected abstract void OnDetach(UIElement element);

    public void AttachTo(UIElement element) {
        if (_parent != null) {
            throw new InvalidOperationException("UI component already attached to an element.");
        }

        _parent = new WeakReference<UIElement>(element);

        OnAttach(element);
    }

    public void DetachFrom(UIElement element) {
        if (_parent == null) {
            throw new InvalidOperationException("UI component not attached to an element.");
        }

        if (element != Parent) {
            throw new InvalidOperationException("Attempted to detach a UI component from an element it is not attached to.");
        }

        OnDetach(element);

        _parent = null!;
    }
}

internal static class UIComponentExtensions {
    public static TComponent AddComponent<TComponent>(this UIElement element, TComponent component) where TComponent : UIComponent {
        component.AttachTo(element);
        return component;
    }
}