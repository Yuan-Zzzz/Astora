using Astora.Core.Nodes;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scripts;

/// <summary>
/// Builds the demo UI tree for interactive testing: UIRoot with BoxContainer and Panels.
/// </summary>
public static class SandboxUIDemo
{
    public static void BuildDemoUI(Node root)
    {
        if (root == null) return;

        var uiRoot = new UIRoot();
        uiRoot.SetViewportFromDesignResolution();
        root.AddChild(uiRoot);

        var box = new BoxContainer { Vertical = true, Spacing = 8 };
        uiRoot.AddChild(box);

        var topPanel = new Panel("TopPanel")
        {
            Size = new Vector2(400, 60),
            Modulate = new Color(60, 120, 180, 240)
        };
        var midPanel = new ClickablePanel("MidPanel")
        {
            Size = new Vector2(400, 80),
            Modulate = new Color(80, 180, 120, 220)
        };
        var bottomPanel = new Panel("BottomPanel")
        {
            Size = new Vector2(400, 100),
            Modulate = new Color(180, 100, 80, 230)
        };

        box.AddChild(topPanel);
        box.AddChild(midPanel);
        box.AddChild(bottomPanel);
    }
}
