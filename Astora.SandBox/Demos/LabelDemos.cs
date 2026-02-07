using Astora.Core.Nodes;
using Astora.Core.Resources;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Astora.Core.UI.Text;
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

    /// <summary>Label effects: shadow, outline, rich text, BBCode, animation.</summary>
    public static void BuildLabelEffects(Node root)
    {
        var font = ResourceLoader.Load<FontResource>(FontPath);
        var box = new BoxContainer { Vertical = true, Spacing = 16 };
        root.AddChild(box);

        var shadow = new Label("Shadow")
        {
            FontResource = font,
            FontSize = 24,
            Text = "Shadow text",
            Modulate = Color.White,
            ShadowColor = new Color(0, 0, 0, 120),
            ShadowOffset = new Vector2(2, 2)
        };
        box.AddChild(shadow);

        var outlineBlack = new Label("OutlineBlack")
        {
            FontResource = font,
            FontSize = 24,
            Text = "Black outline",
            Modulate = Color.White,
            OutlineColor = Color.Black,
            OutlineThickness = 2
        };
        box.AddChild(outlineBlack);

        var outlineRed = new Label("OutlineRed")
        {
            FontResource = font,
            FontSize = 22,
            Text = "Red outline",
            Modulate = Color.White,
            OutlineColor = Color.Red,
            OutlineThickness = 2
        };
        box.AddChild(outlineRed);

        var outlineBlue = new Label("OutlineBlue")
        {
            FontResource = font,
            FontSize = 22,
            Text = "Blue outline",
            Modulate = Color.White,
            OutlineColor = new Color(60, 120, 255, 255),
            OutlineThickness = 2
        };
        box.AddChild(outlineBlue);

        var outlineGreen = new Label("OutlineGreen")
        {
            FontResource = font,
            FontSize = 22,
            Text = "Green outline",
            Modulate = Color.White,
            OutlineColor = new Color(40, 180, 80, 255),
            OutlineThickness = 2
        };
        box.AddChild(outlineGreen);

        var rich = new Label("RichText")
        {
            FontResource = font,
            FontSize = 20,
            RichText = true,
            Text = "Plain /c[red]red/cd and /c[#0080ff]blue/cd /nnew line"
        };
        box.AddChild(rich);

        var bbcode = new Label("BBCode")
        {
            FontResource = font,
            FontSize = 20,
            RichText = true,
            UseBBCode = true,
            Text = "[color=#008000]Green[/color] [b]bold[/b] [color=red]red[/color]"
        };
        box.AddChild(bbcode);

        var anim = new Label("Animation")
        {
            FontResource = font,
            FontSize = 22,
            RichText = true,
            UseBBCode = true,
            Text = "[rainbow]Rainbow[/rainbow] [wave]Wave[/wave]"
        };
        box.AddChild(anim);
    }
}
