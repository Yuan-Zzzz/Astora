using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Button with click for interactive UI testing.</summary>
public class ButtonClickScene : IScene
{
    public static string ScenePath => "Scenes/ButtonClick";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("ButtonClick")
            .Add<Camera2D>("MainCamera")
            .Build();

        var box = new BoxContainer { Name = "Layout", Vertical = true, Spacing = 12 };
        root.AddChild(box);

        var button = new Button("ClickMe")
        {
            Size = new Vector2(200, 50),
            Modulate = new Color(80, 160, 220, 255)
        };
        var clickCount = 0;
        button.Click += () => clickCount++;
        box.AddChild(button);

        var panel = new Panel("Feedback")
        {
            Size = new Vector2(200, 40),
            Modulate = new Color(60, 60, 80, 200)
        };
        box.AddChild(panel);

        return root;
    }
}
