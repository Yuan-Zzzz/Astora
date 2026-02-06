using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.UI;

/// <summary>
/// Displays text (Godot Label style). Uses Size for layout when set; otherwise uses Font.MeasureString(Text) when Font is set.
/// </summary>
public class Label : Control
{
    private string _text = "";
    private SpriteFont? _font;

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

    /// <summary>When set, used to measure text and draw. When null, only explicit Size is used for layout.</summary>
    public SpriteFont? Font
    {
        get => _font;
        set
        {
            if (_font == value) return;
            _font = value;
            InvalidateLayout();
        }
    }

    public Label() : base("Label") { }

    public Label(string name) : base(name) { }

    public override Vector2 ComputeDesiredSize()
    {
        if (Size.X >= 0 && Size.Y >= 0)
        {
            DesiredSize = Size;
            return DesiredSize;
        }
        if (_font != null && !string.IsNullOrEmpty(_text))
        {
            var measure = _font.MeasureString(_text);
            DesiredSize = new Vector2((int)Math.Ceiling(measure.X), (int)Math.Ceiling(measure.Y));
            return DesiredSize;
        }
        DesiredSize = Vector2.Zero;
        return DesiredSize;
    }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        if (!Visible) return;
        if (_font == null || string.IsNullOrEmpty(_text)) return;
        var r = FinalRect;
        renderBatcher.DrawString(_font, _text, new Vector2(r.X, r.Y), Modulate);
    }
}
