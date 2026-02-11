using System.Linq;
using Astora.Core.Attributes;
using Astora.Core.Nodes;
using Astora.Core.UI.Layout;
using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Container;

/// <summary>
/// Container that arranges children in a row (horizontal) or column (vertical). Two-pass layout.
/// </summary>
public class BoxContainer : Control
{
    [SerializeField] private bool _vertical = true;
    [SerializeField] private int _spacing = 4;

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
            float main = _vertical ? desired.Y : desired.X;
            float cross = _vertical ? desired.X : desired.Y;
            float minMain = _vertical ? c.MinSize.Y : c.MinSize.X;
            float minCross = _vertical ? c.MinSize.X : c.MinSize.Y;
            mainSum += Math.Max(main, minMain);
            if (cross > crossMax) crossMax = cross;
            if (minCross > crossMax) crossMax = minCross;
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

        int n = controls.Count;
        float totalStretch = 0f;
        foreach (var c in controls)
            totalStretch += c.StretchRatio;

        if (_vertical)
        {
            int availableMain = finalRect.Height - _spacing * (n - 1);
            float sumBase = 0f;
            foreach (var c in controls)
            {
                float baseMain = Math.Max(c.DesiredSize.Y, c.MinSize.Y);
                sumBase += baseMain;
            }
            float extra = Math.Max(0, availableMain - sumBase);
            int y = finalRect.Y;
            foreach (var child in controls)
            {
                float baseMain = Math.Max(child.DesiredSize.Y, child.MinSize.Y);
                float main = baseMain;
                if (extra > 0 && totalStretch > 0 && child.StretchRatio > 0)
                    main += extra * (child.StretchRatio / totalStretch);
                int h = (int)main;
                int w = finalRect.Width;
                child.ArrangeChildren(new Rectangle(finalRect.X, y, w, h));
                y += h + _spacing;
            }
        }
        else
        {
            int availableMain = finalRect.Width - _spacing * (n - 1);
            float sumBase = 0f;
            foreach (var c in controls)
            {
                float baseMain = Math.Max(c.DesiredSize.X, c.MinSize.X);
                sumBase += baseMain;
            }
            float extra = Math.Max(0, availableMain - sumBase);
            int x = finalRect.X;
            foreach (var child in controls)
            {
                float baseMain = Math.Max(child.DesiredSize.X, child.MinSize.X);
                float main = baseMain;
                if (extra > 0 && totalStretch > 0 && child.StretchRatio > 0)
                    main += extra * (child.StretchRatio / totalStretch);
                int w = (int)main;
                int h = finalRect.Height;
                child.ArrangeChildren(new Rectangle(x, finalRect.Y, w, h));
                x += w + _spacing;
            }
        }
    }
}
