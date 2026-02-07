using System.Reflection;
using Astora.Core.UI;
using Astora.Core.UI.Events;
using Astora.Core.UI.Rendering;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class ButtonTests
{
    private static readonly Rectangle TestViewport = new Rectangle(0, 0, 100, 100);

    private static void RouteMouseButtonDown(Control root, Control? hitTarget, MouseButtonEventArgs args)
    {
        typeof(Control).GetMethod("RouteMouseButtonDown", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(root, new object?[] { hitTarget, args });
    }

    private static void RouteMouseButtonUp(Control root, Control? hitTarget, MouseButtonEventArgs args)
    {
        typeof(Control).GetMethod("RouteMouseButtonUp", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(root, new object?[] { hitTarget, args });
    }

    [Fact]
    public void Click_FiresWhenPressedThenReleasedOnSameControl()
    {
        var root = new Control();
        var button = new Button { Size = new Vector2(50, 30) };
        root.AddChild(button);
        root.DoLayout(TestViewport);

        var clicked = 0;
        button.Click += () => clicked++;

        var downArgs = new MouseButtonEventArgs { Position = new Vector2(25, 15), Button = MouseButton.Left, Pressed = true };
        RouteMouseButtonDown(root, button, downArgs);
        downArgs.Handled.Should().BeTrue();

        var upArgs = new MouseButtonEventArgs { Position = new Vector2(25, 15), Button = MouseButton.Left, Pressed = false };
        RouteMouseButtonUp(root, button, upArgs);
        upArgs.Handled.Should().BeTrue();
        clicked.Should().Be(1);
    }

    [Fact]
    public void Click_DoesNotFireWhenDisabled()
    {
        var root = new Control();
        var button = new Button { Size = new Vector2(50, 30), Disabled = true };
        root.AddChild(button);
        root.DoLayout(TestViewport);

        var clicked = 0;
        button.Click += () => clicked++;

        RouteMouseButtonDown(root, button, new MouseButtonEventArgs { Position = Vector2.One, Button = MouseButton.Left, Pressed = true });
        RouteMouseButtonUp(root, button, new MouseButtonEventArgs { Position = Vector2.One, Button = MouseButton.Left, Pressed = false });

        clicked.Should().Be(0);
    }

    [Fact]
    public void Draw_WhenUIDrawContextNull_DoesNotThrow()
    {
        UIDrawContext.SetCurrent(null);
        var button = new Button { Size = new Vector2(50, 50) };
        button.ArrangeChildren(new Rectangle(0, 0, 50, 50));
        button.Invoking(b => b.Draw(null!)).Should().NotThrow();
    }

    [Fact]
    public void Draw_WhenVisibleFalse_DoesNotThrow()
    {
        var button = new Button { Visible = false, Size = new Vector2(10, 10) };
        button.ArrangeChildren(new Rectangle(0, 0, 10, 10));
        button.Invoking(b => b.Draw(null!)).Should().NotThrow();
    }
}
