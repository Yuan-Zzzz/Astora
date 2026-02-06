using System.Reflection;
using Astora.Core.UI;
using Astora.Core.UI.Events;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class UIRootTests
{
    [Fact]
    public void DoLayout_SetsViewportAndRunsTwoPass()
    {
        var root = new UIRoot();
        root.ViewportWidth = 800;
        root.ViewportHeight = 600;
        root.DoLayout();

        root.FinalRect.Should().Be(new Rectangle(0, 0, 800, 600));
        root.IsLayoutDirty.Should().BeFalse();
    }

    [Fact]
    public void HitTest_ReturnsNull_WhenPointOutside()
    {
        var root = new UIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        root.DoLayout();

        var hit = root.HitTest(new Vector2(150, 50));
        hit.Should().BeNull();
    }

    [Fact]
    public void HitTest_ReturnsRoot_WhenPointInsideAndNoChildren()
    {
        var root = new UIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        root.DoLayout();

        var hit = root.HitTest(new Vector2(50, 50));
        hit.Should().Be(root);
    }

    [Fact]
    public void HitTest_ReturnsChild_WhenPointInsideChild()
    {
        var root = new UIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        var child = new Control { Size = new Vector2(50, 50) };
        root.AddChild(child);
        root.DoLayout();

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(child);
    }

    [Fact]
    public void HitTest_IgnoresInvisibleControl()
    {
        var root = new UIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        var child = new Control { Size = new Vector2(50, 50), Visible = false };
        root.AddChild(child);
        root.DoLayout();

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(root);
    }

    [Fact]
    public void HitTest_IgnoresMouseFilterIgnore()
    {
        var root = new UIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        var child = new Control { Size = new Vector2(50, 50), MouseFilter = MouseFilter.Ignore };
        root.AddChild(child);
        root.DoLayout();

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(root);
    }

    [Fact]
    public void HitTest_MouseFilterStop_DoesNotRecurseIntoChildren()
    {
        var root = new UIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        var panel = new Control { Size = new Vector2(100, 100), MouseFilter = MouseFilter.Stop };
        var inner = new Control { Size = new Vector2(50, 50) };
        panel.AddChild(inner);
        root.AddChild(panel);
        root.DoLayout();

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(panel);
    }

    [Fact]
    public void Route_Handled_StopsBubbling()
    {
        var root = new TestUIRoot();
        root.ViewportWidth = 100;
        root.ViewportHeight = 100;
        var child = new TestControlThatHandles { Size = new Vector2(50, 50) };
        root.AddChild(child);
        root.DoLayout();

        var args = new MouseButtonEventArgs { Position = new Vector2(25, 25), Button = MouseButton.Left, Pressed = true };
        root.RouteMouseButtonDownPublic(child, args);

        args.Handled.Should().BeTrue();
        root.BubblingReceived.Should().BeFalse();
    }
}

internal class TestUIRoot : UIRoot
{
    public bool BubblingReceived { get; private set; }
    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        BubblingReceived = true;
    }
    public void RouteMouseButtonDownPublic(Control? hitTarget, MouseButtonEventArgs args) =>
        typeof(UIRoot).GetMethod("RouteMouseButtonDown", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(this, new object?[] { hitTarget, args });
}

internal class TestControlThatHandles : Control
{
    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
