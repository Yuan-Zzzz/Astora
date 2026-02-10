using Astora.Core.Nodes;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Astora.Core.UI.Text;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Button with Label child and click feedback.</summary>
public class LabelButtonScene : IScene
{
    public static string ScenePath => "Scenes/LabelButton";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("LabelButton")
            .Add<Camera2D>("MainCamera")
            .Build();

        var font = ResourceLoader.Load<FontResource>("Fonts/f.ttf");

        var box = new BoxContainer { Name = "Layout", Vertical = true, Spacing = 16 };
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

        return root;
    }
}
