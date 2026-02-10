using Astora.Core.Nodes;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Astora.Core.UI;
using Astora.Core.UI.Container;
using Astora.Core.UI.Text;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scenes;

/// <summary>Demo scene: Label effects (shadow, outline, rich text, BBCode, animation).</summary>
public class LabelEffectsScene : IScene
{
    public static string ScenePath => "Scenes/LabelEffects";

    public static Node Build()
    {
        var root = SceneBuilder.Create<Node>("LabelEffects")
            .Add<Camera2D>("MainCamera")
            .Build();

        var font = ResourceLoader.Load<FontResource>("Fonts/f.ttf");

        var box = new BoxContainer { Name = "EffectsList", Vertical = true, Spacing = 16 };
        root.AddChild(box);

        box.AddChild(new Label("Shadow")
        {
            FontResource = font, FontSize = 24,
            Text = "Shadow text", Modulate = Color.White,
            ShadowColor = new Color(0, 0, 0, 120),
            ShadowOffset = new Vector2(2, 2)
        });

        box.AddChild(new Label("OutlineBlack")
        {
            FontResource = font, FontSize = 24,
            Text = "Black outline", Modulate = Color.White,
            OutlineColor = Color.Black, OutlineThickness = 2
        });

        box.AddChild(new Label("OutlineRed")
        {
            FontResource = font, FontSize = 22,
            Text = "Red outline", Modulate = Color.White,
            OutlineColor = Color.Red, OutlineThickness = 2
        });

        box.AddChild(new Label("OutlineBlue")
        {
            FontResource = font, FontSize = 22,
            Text = "Blue outline", Modulate = Color.White,
            OutlineColor = new Color(60, 120, 255, 255), OutlineThickness = 2
        });

        box.AddChild(new Label("OutlineGreen")
        {
            FontResource = font, FontSize = 22,
            Text = "Green outline", Modulate = Color.White,
            OutlineColor = new Color(40, 180, 80, 255), OutlineThickness = 2
        });

        box.AddChild(new Label("RichText")
        {
            FontResource = font, FontSize = 20,
            RichText = true,
            Text = "Plain /c[red]red/cd and /c[#0080ff]blue/cd /nnew line"
        });

        box.AddChild(new Label("BBCode")
        {
            FontResource = font, FontSize = 20,
            RichText = true, UseBBCode = true,
            Text = "[color=#008000]Green[/color] [b]bold[/b] [color=red]red[/color]"
        });

        box.AddChild(new Label("Animation")
        {
            FontResource = font, FontSize = 22,
            RichText = true, UseBBCode = true,
            Text = "[rainbow]Rainbow[/rainbow] [wave]Wave[/wave]"
        });

        return root;
    }
}
