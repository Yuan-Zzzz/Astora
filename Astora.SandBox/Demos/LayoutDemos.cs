using Astora.Core.Nodes;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Demos;

/// <summary>
/// Layout demos: BoxContainer, MarginContainer, StretchRatio. No font dependency.
/// </summary>
public static class LayoutDemos
{
    /// <summary>Vertical BoxContainer with three panels.</summary>
    public static void BuildBoxContainerVertical(Node root)
    {
        var box = new BoxContainer { Vertical = true, Spacing = 8 };
        root.AddChild(box);

        var top = new Panel("Top") { Size = new Vector2(400, 60), Modulate = new Color(60, 120, 180, 240) };
        var mid = new Panel("Mid") { Size = new Vector2(400, 80), Modulate = new Color(80, 180, 120, 220) };
        var bottom = new Panel("Bottom") { Size = new Vector2(400, 100), Modulate = new Color(180, 100, 80, 230) };
        box.AddChild(top);
        box.AddChild(mid);
        box.AddChild(bottom);
    }

    /// <summary>MarginContainer with a single centered panel.</summary>
    public static void BuildMarginContainer(Node root)
    {
        var margin = new MarginContainer();
        margin.SetMarginAll(40);
        root.AddChild(margin);

        var panel = new Panel("Inner") { Size = new Vector2(300, 200), Modulate = new Color(100, 100, 150, 250) };
        margin.AddChild(panel);
    }

    /// <summary>Horizontal BoxContainer with fixed and stretch children.</summary>
    public static void BuildBoxContainerHorizontalWithStretch(Node root)
    {
        var box = new BoxContainer { Vertical = false, Spacing = 4 };
        root.AddChild(box);

        var left = new Panel("Left") { Size = new Vector2(80, 60), Modulate = new Color(200, 100, 100, 255) };
        var center = new Panel("Center") { Size = new Vector2(0, 60), StretchRatio = 1f, Modulate = new Color(100, 200, 100, 255) };
        var right = new Panel("Right") { Size = new Vector2(80, 60), Modulate = new Color(100, 100, 200, 255) };
        box.AddChild(left);
        box.AddChild(center);
        box.AddChild(right);
    }
}
