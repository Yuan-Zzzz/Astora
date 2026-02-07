using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Resources;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Astora.Core.UI;

/// <summary>
/// Displays text (Godot Label style). Uses Size for layout when set;
/// otherwise uses FontResource.GetFont(FontSize).MeasureString(Text).
/// Font is resolved via FontResource property, falling back to Theme "default_font".
/// </summary>
public class Label : Control
{
    private string _text = "";
    private FontResource? _fontResource;
    private float _fontSize = 16f;

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value ?? "";
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Font resource that provides the underlying FontSystem.
    /// When null, falls back to Theme "default_font".
    /// </summary>
    public FontResource? FontResource
    {
        get => _fontResource;
        set
        {
            if (_fontResource == value) return;
            _fontResource = value;
            InvalidateLayout();
        }
    }

    /// <summary>Font size in pixels. Default 16.</summary>
    public float FontSize
    {
        get => _fontSize;
        set
        {
            if (Math.Abs(_fontSize - value) < 0.01f) return;
            _fontSize = value;
            InvalidateLayout();
        }
    }

    public Label() : base("Label") { }

    public Label(string name) : base(name) { }

    /// <summary>
    /// Resolves the drawable font: explicit FontResource, then Theme "default_font".
    /// </summary>
    private SpriteFontBase? GetFont()
    {
        var resource = _fontResource ?? GetThemeFont("default_font");
        return resource?.GetFont(_fontSize);
    }

    public override Vector2 ComputeDesiredSize()
    {
        if (Size.X >= 0 && Size.Y >= 0)
        {
            DesiredSize = Size;
            return DesiredSize;
        }

        var font = GetFont();
        if (font != null && !string.IsNullOrEmpty(_text))
        {
            var measure = font.MeasureString(_text);
            DesiredSize = new Vector2((int)Math.Ceiling(measure.X), (int)Math.Ceiling(measure.Y));
            return DesiredSize;
        }

        DesiredSize = Vector2.Zero;
        return DesiredSize;
    }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        if (!Visible) return;
        var font = GetFont();
        if (font == null || string.IsNullOrEmpty(_text)) return;
        var r = FinalRect;
        renderBatcher.DrawString(font, _text, new Vector2(r.X, r.Y), Modulate);
    }
}
