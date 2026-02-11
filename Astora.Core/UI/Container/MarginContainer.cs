using System.Linq;
using Astora.Core.Attributes;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Container;

/// <summary>
/// Single-child container that insets the child by margins (Godot MarginContainer style).
/// </summary>
public class MarginContainer : Control
{
    [SerializeField] private int _marginLeft;
    [SerializeField] private int _marginTop;
    [SerializeField] private int _marginRight;
    [SerializeField] private int _marginBottom;

    public int MarginLeft
    {
        get => _marginLeft;
        set { if (_marginLeft == value) return; _marginLeft = value; InvalidateLayout(); }
    }

    public int MarginTop
    {
        get => _marginTop;
        set { if (_marginTop == value) return; _marginTop = value; InvalidateLayout(); }
    }

    public int MarginRight
    {
        get => _marginRight;
        set { if (_marginRight == value) return; _marginRight = value; InvalidateLayout(); }
    }

    public int MarginBottom
    {
        get => _marginBottom;
        set { if (_marginBottom == value) return; _marginBottom = value; InvalidateLayout(); }
    }

    /// <summary>Set all four margins to the same value.</summary>
    public void SetMarginAll(int margin)
    {
        MarginLeft = MarginTop = MarginRight = MarginBottom = margin;
    }

    public MarginContainer() : base("MarginContainer") { }

    public MarginContainer(string name) : base(name) { }

    public override Vector2 ComputeDesiredSize()
    {
        var child = Children.OfType<Control>().FirstOrDefault();
        if (child == null)
        {
            DesiredSize = new Vector2(_marginLeft + _marginRight, _marginTop + _marginBottom);
            return DesiredSize;
        }
        var desired = child.ComputeDesiredSize();
        child.DesiredSize = desired;
        float w = desired.X + _marginLeft + _marginRight;
        float h = desired.Y + _marginTop + _marginBottom;
        DesiredSize = new Vector2(w, h);
        return DesiredSize;
    }

    public override void ArrangeChildren(Rectangle finalRect)
    {
        base.ArrangeChildren(finalRect);
        var child = Children.OfType<Control>().FirstOrDefault();
        if (child == null) return;
        int x = finalRect.X + _marginLeft;
        int y = finalRect.Y + _marginTop;
        int w = finalRect.Width - _marginLeft - _marginRight;
        int h = finalRect.Height - _marginTop - _marginBottom;
        if (w < 0) w = 0;
        if (h < 0) h = 0;
        child.ArrangeChildren(new Rectangle(x, y, w, h));
    }
}
