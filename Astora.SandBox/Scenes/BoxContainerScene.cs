using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Vertical BoxContainer with three panels.</summary>
public class BoxContainerScene : IScene
{
    public static string ScenePath => "Scenes/BoxContainer";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("BoxContainer")
            .Add<Camera2D>("MainCamera")
            .Build();

        var box = new BoxContainer { Name = "VBox", Vertical = true, Spacing = 8 };
        root.AddChild(box);

        box.AddChild(new Panel("Top") { Size = new Vector2(400, 60), Modulate = new Color(60, 120, 180, 240) });
        box.AddChild(new Panel("Mid") { Size = new Vector2(400, 80), Modulate = new Color(80, 180, 120, 220) });
        box.AddChild(new Panel("Bottom") { Size = new Vector2(400, 100), Modulate = new Color(180, 100, 80, 230) });

        return root;
    }
}
