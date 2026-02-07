using Astora.Core.Nodes;
using Astora.Core.Resources;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Demos;

/// <summary>
/// Label demos: font rendering with different sizes and layouts.
/// </summary>
public static class LabelDemos
{
    private const string FontPath = "Content/Fonts/f.ttf";

    /// <summary>Labels at various font sizes inside a vertical BoxContainer.</summary>
    public static void BuildFontSizes(Node root)
    {
        var font = ResourceLoader.Load<FontResource>(FontPath);

        var box = new BoxContainer { Vertical = true, Spacing = 12 };
        root.AddChild(box);

        var sizes = new[] { 16f, 24f, 32f, 48f };
        foreach (var size in sizes)
        {
            var label = new Label($"Label_{size}pt")
            {
                FontResource = font,
                FontSize = size,
                Text = $"Astora Engine — {size}px",
                Modulate = Color.Black
            };
            box.AddChild(label);
        }
    }

    /// <summary>Button with a Label child, demonstrating mixed UI with text.</summary>
    public static void BuildButtonWithLabel(Node root)
    {
        var font = ResourceLoader.Load<FontResource>(FontPath);

        var box = new BoxContainer { Vertical = true, Spacing = 16 };
        root.AddChild(box);

        var button = new Button("TextButton")
        {
            Size = new Vector2(280, 50),
            Modulate = new Color(60, 130, 200, 255)
        };
        box.AddChild(button);

        var label = new Label("ButtonLabel")
        {
            FontResource = font,
            FontSize = 24,
            Text = "Click Me!",
            Modulate = Color.White
        };
        button.AddChild(label);

        var feedback = new Label("Feedback")
        {
            FontResource = font,
            FontSize = 20,
            Text = "点击按钮试试 / Click the button",
            Modulate = Color.Black
        };
        box.AddChild(feedback);

        var clickCount = 0;
        button.Click += () =>
        {
            clickCount++;
            feedback.Text = $"Clicked {clickCount} time(s)";
        };
    }
}
