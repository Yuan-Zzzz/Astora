using Astora.Core.Nodes;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Demos;

/// <summary>
/// Interaction demos: Button click, multiple buttons. No font dependency.
/// </summary>
public static class InteractionDemos
{
    /// <summary>Single button that changes color on click (visual feedback for interactive testing).</summary>
    public static void BuildButtonClick(Node root)
    {
        var box = new BoxContainer { Vertical = true, Spacing = 12 };
        root.AddChild(box);

        var button = new Button("ClickMe")
        {
            Size = new Vector2(200, 50),
            Modulate = new Color(80, 160, 220, 255)
        };
        var clickCount = 0;
        button.Click += () => clickCount++;
        box.AddChild(button);

        var panel = new Panel("Feedback") { Size = new Vector2(200, 40), Modulate = new Color(60, 60, 80, 200) };
        box.AddChild(panel);
    }

    /// <summary>Row of buttons for testing focus and hit-testing.</summary>
    public static void BuildMultipleButtons(Node root)
    {
        var box = new BoxContainer { Vertical = false, Spacing = 8 };
        root.AddChild(box);

        for (int i = 0; i < 4; i++)
        {
            var btn = new Button($"Button{i + 1}")
            {
                Size = new Vector2(120, 44),
                Modulate = new Color(70 + i * 30, 120, 180, 255)
            };
            box.AddChild(btn);
        }
    }
}
