using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: MarginContainer with a single centered panel.</summary>
public class MarginContainerScene : IScene
{
    public static string ScenePath => "Scenes/MarginContainer";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("MarginContainer")
            .Add<Camera2D>("MainCamera")
            .Build();

        var margin = new MarginContainer { Name = "Margin" };
        margin.SetMarginAll(40);
        root.AddChild(margin);

        margin.AddChild(new Panel("Inner")
        {
            Size = new Vector2(300, 200),
            Modulate = new Color(100, 100, 150, 250)
        });

        return root;
    }
}
