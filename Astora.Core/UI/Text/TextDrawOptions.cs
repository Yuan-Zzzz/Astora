using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Text;

/// <summary>
/// Options for multi-pass text drawing: shadow and outline.
/// When ShadowColor is set, text is drawn at position + ShadowOffset first.
/// When OutlineColor is set and OutlineThickness > 0, text is drawn in 8 directions then filled.
/// </summary>
public struct TextDrawOptions
{
    /// <summary>Offset for shadow draw pass. Used when ShadowColor is set.</summary>
    public Vector2 ShadowOffset;

    /// <summary>When set, a shadow pass is drawn at position + ShadowOffset.</summary>
    public Color? ShadowColor;

    /// <summary>When set and OutlineThickness > 0, outline passes are drawn before fill.</summary>
    public Color? OutlineColor;

    /// <summary>Outline thickness in pixels. Ignored when 0 or OutlineColor is null.</summary>
    public int OutlineThickness;

    /// <summary>No shadow, no outline.</summary>
    public static TextDrawOptions None => default;

    /// <summary>Options with only shadow.</summary>
    public static TextDrawOptions WithShadow(Vector2 offset, Color shadowColor) => new TextDrawOptions
    {
        ShadowOffset = offset,
        ShadowColor = shadowColor
    };

    /// <summary>Options with only outline.</summary>
    public static TextDrawOptions WithOutline(Color outlineColor, int thickness) => new TextDrawOptions
    {
        OutlineColor = outlineColor,
        OutlineThickness = thickness
    };
}
