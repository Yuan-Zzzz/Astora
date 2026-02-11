using Astora.Core.Attributes;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Resources;
using Astora.Core.UI.Text;
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
    [SerializeField] private string _text = "";
    private FontResource? _fontResource;
    [SerializeField] private float _fontSize = 16f;

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

    [SerializeField] private Vector2 _shadowOffset;
    [SerializeField] private Color? _shadowColor;

    /// <summary>Offset for shadow. Used when ShadowColor is set.</summary>
    public Vector2 ShadowOffset
    {
        get => _shadowOffset;
        set => _shadowOffset = value;
    }

    /// <summary>When set, text is drawn with a shadow at position + ShadowOffset.</summary>
    public Color? ShadowColor
    {
        get => _shadowColor;
        set => _shadowColor = value;
    }

    [SerializeField] private Color? _outlineColor;
    [SerializeField] private int _outlineThickness;

    [SerializeField] private bool _richText;

    /// <summary>When true, Text is interpreted as FSS rich text commands (/c[color], /n, etc.).</summary>
    public bool RichText
    {
        get => _richText;
        set
        {
            if (_richText == value) return;
            _richText = value;
            InvalidateLayout();
        }
    }

    [SerializeField] private bool _useBBCode;

    /// <summary>When true and RichText is true, Text is parsed as BBCode ([color=], [b], etc.) and converted to FSS commands.</summary>
    public bool UseBBCode
    {
        get => _useBBCode;
        set
        {
            if (_useBBCode == value) return;
            _useBBCode = value;
            InvalidateLayout();
        }
    }

    [SerializeField] private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;

    /// <summary>Horizontal alignment of text within the label's bounds.</summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => _horizontalAlignment = value;
    }

    [SerializeField] private bool _autoEllipsis;
    [SerializeField] private string _ellipsisString = "…";

    /// <summary>When true and RichText and Width/Height are set, overflow is abbreviated with EllipsisString.</summary>
    public bool AutoEllipsis
    {
        get => _autoEllipsis;
        set
        {
            if (_autoEllipsis == value) return;
            _autoEllipsis = value;
            InvalidateLayout();
        }
    }

    /// <summary>String used for ellipsis when AutoEllipsis is true. Default "…".</summary>
    public string EllipsisString
    {
        get => _ellipsisString;
        set => _ellipsisString = value ?? "…";
    }

    private float _animationTime;

    /// <summary>Accumulated time in seconds for BBCode animation tags ([wave], [rainbow], [shake]). Updated in Update.</summary>
    public float AnimationTime => _animationTime;

    /// <summary>When set and OutlineThickness > 0, text is drawn with an outline. Can be any color (e.g. red, blue, transparent).</summary>
    public Color? OutlineColor
    {
        get => _outlineColor;
        set => _outlineColor = value;
    }

    /// <summary>Outline thickness in pixels. No outline when 0.</summary>
    public int OutlineThickness
    {
        get => _outlineThickness;
        set => _outlineThickness = value < 0 ? 0 : value;
    }

    public Label() : base("Label") { }

    public Label(string name) : base(name) { }

    public override void Update(float delta)
    {
        base.Update(delta);
        _animationTime += delta;
    }

    /// <summary>
    /// Resolves the drawable font: explicit FontResource, then Theme "default_font".
    /// </summary>
    private SpriteFontBase? GetFont()
    {
        var resource = _fontResource ?? GetThemeFont("default_font");
        return resource?.GetFont(_fontSize);
    }

    private FontStashSharp.RichText.RichTextLayout BuildRichTextLayout(SpriteFontBase font, string source)
    {
        var layout = new FontStashSharp.RichText.RichTextLayout { Font = font, Text = source };
        if (_autoEllipsis && Size.X >= 0 && Size.Y >= 0)
        {
            layout.Width = (int)Size.X;
            layout.Height = (int)Size.Y;
            layout.AutoEllipsisMethod = FontStashSharp.RichText.AutoEllipsisMethod.Character;
            layout.AutoEllipsisString = _ellipsisString;
        }
        return layout;
    }

    public override Vector2 ComputeDesiredSize()
    {
        if (Size.X >= 0 && Size.Y >= 0)
        {
            DesiredSize = Size;
            return DesiredSize;
        }

        var font = GetFont();
        if (font == null || string.IsNullOrEmpty(_text))
        {
            DesiredSize = Vector2.Zero;
            return DesiredSize;
        }

        if (_richText)
        {
            var source = _useBBCode ? BBCodeParser.ToRichTextCommands(_text, _animationTime) : _text;
            var layout = BuildRichTextLayout(font, source);
            var pt = layout.Size;
            DesiredSize = new Vector2(pt.X, pt.Y);
            return DesiredSize;
        }

        var measure = font.MeasureString(_text);
        DesiredSize = new Vector2((int)Math.Ceiling(measure.X), (int)Math.Ceiling(measure.Y));
        return DesiredSize;
    }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        if (!Visible) return;
        var font = GetFont();
        if (font == null || string.IsNullOrEmpty(_text)) return;
        var r = FinalRect;
        float textWidth = 0;
        if (!_richText)
        {
            var measure = font.MeasureString(_text);
            textWidth = measure.X;
        }
        var pos = _horizontalAlignment switch
        {
            HorizontalAlignment.Center when !_richText => new Vector2(r.X + (r.Width - textWidth) * 0.5f, r.Y),
            HorizontalAlignment.Right when !_richText => new Vector2(r.X + (r.Width - textWidth), r.Y),
            _ => new Vector2(r.X, r.Y)
        };

        if (_richText)
        {
            var source = _useBBCode ? BBCodeParser.ToRichTextCommands(_text, _animationTime) : _text;
            var layout = BuildRichTextLayout(font, source);
            var alignPos = _horizontalAlignment switch
            {
                HorizontalAlignment.Center => new Vector2(r.X + r.Width * 0.5f, r.Y),
                HorizontalAlignment.Right => new Vector2(r.X + r.Width, r.Y),
                _ => pos
            };
            renderBatcher.DrawRichText(layout, alignPos, Modulate, _horizontalAlignment);
            return;
        }

        var options = new TextDrawOptions
        {
            ShadowOffset = _shadowOffset,
            ShadowColor = _shadowColor,
            OutlineColor = _outlineColor,
            OutlineThickness = _outlineThickness
        };
        if (options.ShadowColor.HasValue || (options.OutlineColor.HasValue && options.OutlineThickness > 0))
            renderBatcher.DrawString(font, _text, pos, Modulate, options);
        else
            renderBatcher.DrawString(font, _text, pos, Modulate);
    }
}
