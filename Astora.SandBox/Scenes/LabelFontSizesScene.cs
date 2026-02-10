using Astora.Core.Nodes;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Astora.Core.UI.Text;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Label font rendering at multiple sizes.</summary>
public class LabelFontSizesScene : IScene
{
    public static string ScenePath => "Scenes/LabelFontSizes";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("LabelFontSizes")
            .Add<Camera2D>("MainCamera")
            .Build();

        var font = ResourceLoader.Load<FontResource>("Fonts/f.ttf");

        var box = new BoxContainer { Name = "FontSizeList", Vertical = true, Spacing = 12 };
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

        return root;
    }
}
