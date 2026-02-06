using System.Linq;
using Astora.Core.Nodes;
using Astora.Core.UI.Layout;
using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Container;

/// <summary>
/// Container that arranges children in a row (horizontal) or column (vertical). Two-pass layout.
/// </summary>
public class BoxContainer : Control
{
    private bool _vertical = true;
    private int _spacing = 4;

    /// <summary>
    /// True for vertical (column), false for horizontal (row).
    /// </summary>
    public bool Vertical
    {
        get => _vertical;
        set
        {
            if (_vertical == value) return;
            _vertical = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Space between children in pixels.
    /// </summary>
    public int Spacing
    {
        get => _spacing;
        set
        {
            if (_spacing == value) return;
            _spacing = value;
            InvalidateLayout();
        }
    }

    public BoxContainer() : base("BoxContainer") { }

    public BoxContainer(string name) : base(name) { }

    public override Vector2 ComputeDesiredSize()
    {
        var controls = Children.OfType<Control>().ToList();
        if (controls.Count == 0)
        {
            DesiredSize = Vector2.Zero;
            return DesiredSize;
        }

        float mainSum = 0;
        float crossMax = 0;
        int n = controls.Count;
        foreach (var c in controls)
        {
            var desired = c.ComputeDesiredSize();
            c.DesiredSize = desired;
            if (_vertical)
            {
                mainSum += desired.Y;
                if (desired.X > crossMax) crossMax = desired.X;
            }
            else
            {
                mainSum += desired.X;
                if (desired.Y > crossMax) crossMax = desired.Y;
            }
        }
        float totalMain = mainSum + _spacing * (n - 1);
        if (_vertical)
            DesiredSize = new Vector2(crossMax, totalMain);
        else
            DesiredSize = new Vector2(totalMain, crossMax);
        return DesiredSize;
    }

    public override void ArrangeChildren(Rectangle finalRect)
    {
        base.ArrangeChildren(finalRect);
        var controls = Children.OfType<Control>().ToList();
        if (controls.Count == 0) return;

        int x = finalRect.X;
        int y = finalRect.Y;
        foreach (var child in controls)
        {
            int w, h;
            if (_vertical)
            {
                w = finalRect.Width;
                h = (int)child.DesiredSize.Y;
                child.ArrangeChildren(new Rectangle(x, y, w, h));
                y += h + _spacing;
            }
            else
            {
                w = (int)child.DesiredSize.X;
                h = finalRect.Height;
                child.ArrangeChildren(new Rectangle(x, y, w, h));
                x += w + _spacing;
            }
        }
    }
}
