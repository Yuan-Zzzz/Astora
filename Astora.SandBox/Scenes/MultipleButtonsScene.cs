using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Row of buttons for focus and hit-testing.</summary>
public class MultipleButtonsScene : IScene
{
    public static string ScenePath => "Scenes/MultipleButtons";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("MultipleButtons")
            .Add<Camera2D>("MainCamera")
            .Build();

        var box = new BoxContainer { Name = "ButtonRow", Vertical = false, Spacing = 8 };
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

        return root;
    }
}
