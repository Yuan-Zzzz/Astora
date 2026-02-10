using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Two CanvasLayers to verify draw and hit order.</summary>
public class LayeringScene : IScene
{
    public static string ScenePath => "Scenes/Layering";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("Layering")
            .Add<Camera2D>("MainCamera")
            .Build();

        var layer0 = new CanvasLayer { Name = "BackLayer", Layer = 0 };
        root.AddChild(layer0);
        layer0.AddChild(new Panel("Layer0Bg")
        {
            Size = new Vector2(800, 600),
            Modulate = new Color(40, 40, 60, 180)
        });

        var layer1 = new CanvasLayer { Name = "FrontLayer", Layer = 1 };
        root.AddChild(layer1);
        var box = new BoxContainer { Name = "FrontBox", Vertical = true, Spacing = 8 };
        layer1.AddChild(box);
        box.AddChild(new Panel("Layer1Top")
        {
            Size = new Vector2(300, 80),
            Modulate = new Color(200, 100, 100, 230)
        });

        return root;
    }
}
