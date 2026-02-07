using Astora.Core.Nodes;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Demos;

/// <summary>
/// Layering demos: CanvasLayer with two layers to verify draw and hit order. No font dependency.
/// </summary>
public static class LayeringDemos
{
    /// <summary>Two CanvasLayers: layer 0 full-screen tint, layer 1 small panel on top (should receive hit first).</summary>
    public static void BuildTwoLayers(Node root)
    {
        var layer0 = new CanvasLayer { Layer = 0 };
        root.AddChild(layer0);
        var bg = new Panel("Layer0Bg") { Size = new Vector2(800, 600), Modulate = new Color(40, 40, 60, 180) };
        layer0.AddChild(bg);

        var layer1 = new CanvasLayer { Layer = 1 };
        root.AddChild(layer1);
        var box = new BoxContainer { Vertical = true, Spacing = 8 };
        layer1.AddChild(box);
        var topPanel = new Panel("Layer1Top") { Size = new Vector2(300, 80), Modulate = new Color(200, 100, 100, 230) };
        box.AddChild(topPanel);
    }
}
