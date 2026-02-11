using Astora.Core.Attributes;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI.Events;
using Astora.Core.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.UI;

/// <summary>
/// Clickable button (Godot BaseButton/Button style). Fires Click when pressed and released on the same control.
/// </summary>
public class Button : Control
{
    private bool _pressed;
    [SerializeField] private bool _disabled;

    /// <summary>When true, the button does not respond to clicks and can be drawn with a disabled style.</summary>
    public bool Disabled
    {
        get => _disabled;
        set => _disabled = value;
    }

    /// <summary>Raised when the button is clicked (mouse pressed then released on this control).</summary>
    public event Action? Click;

    public Button() : base("Button") { FocusMode = FocusMode.Click; }

    public Button(string name) : base(name) { FocusMode = FocusMode.Click; }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        if (!Visible) return;
        var tex = UIDrawContext.Current?.GetWhiteTexture();
        if (tex == null) return;
        var r = FinalRect;
        var color = _disabled ? new Color(120, 120, 120, 200) : (_pressed ? Modulate * 0.8f : Modulate);
        renderBatcher.Draw(
            tex,
            new Vector2(r.X, r.Y),
            new Rectangle(0, 0, 1, 1),
            color,
            0f,
            Vector2.Zero,
            new Vector2(r.Width, r.Height),
            SpriteEffects.None,
            0f
        );
    }

    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        if (_disabled || e.Button != MouseButton.Left) return;
        _pressed = true;
        e.Handled = true;
    }

    public override void OnMouseButtonUp(MouseButtonEventArgs e)
    {
        if (e.Button != MouseButton.Left) return;
        if (_pressed)
        {
            _pressed = false;
            if (!_disabled)
                Click?.Invoke();
            e.Handled = true;
        }
    }
}
