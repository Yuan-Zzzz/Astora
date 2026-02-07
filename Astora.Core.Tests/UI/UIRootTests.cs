using System.Reflection;
using Astora.Core.UI;
using Astora.Core.UI.Events;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

/// <summary>
/// Tests for UI root behaviour (any Control as root; layout/hit test/route driven by SceneTree).
/// </summary>
public class UIRootTests
{
    private static readonly Rectangle TestViewport = new Rectangle(0, 0, 100, 100);

    [Fact]
    public void DoLayout_SetsViewportAndRunsTwoPass()
    {
        var root = new Control();
        root.DoLayout(new Rectangle(0, 0, 800, 600));

        root.FinalRect.Should().Be(new Rectangle(0, 0, 800, 600));
        root.IsLayoutDirty.Should().BeFalse();
    }

    [Fact]
    public void HitTest_ReturnsNull_WhenPointOutside()
    {
        var root = new Control();
        root.DoLayout(TestViewport);

        var hit = root.HitTest(new Vector2(150, 50));
        hit.Should().BeNull();
    }

    [Fact]
    public void HitTest_ReturnsRoot_WhenPointInsideAndNoChildren()
    {
        var root = new Control();
        root.DoLayout(TestViewport);

        var hit = root.HitTest(new Vector2(50, 50));
        hit.Should().Be(root);
    }

    [Fact]
    public void HitTest_ReturnsChild_WhenPointInsideChild()
    {
        var root = new Control();
        var child = new Control { Size = new Vector2(50, 50) };
        root.AddChild(child);
        root.DoLayout(TestViewport);

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(child);
    }

    [Fact]
    public void HitTest_IgnoresInvisibleControl()
    {
        var root = new Control();
        var child = new Control { Size = new Vector2(50, 50), Visible = false };
        root.AddChild(child);
        root.DoLayout(TestViewport);

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(root);
    }

    [Fact]
    public void HitTest_IgnoresMouseFilterIgnore()
    {
        var root = new Control();
        var child = new Control { Size = new Vector2(50, 50), MouseFilter = MouseFilter.Ignore };
        root.AddChild(child);
        root.DoLayout(TestViewport);

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(root);
    }

    [Fact]
    public void HitTest_MouseFilterStop_DoesNotRecurseIntoChildren()
    {
        var root = new Control();
        var panel = new Control { Size = new Vector2(100, 100), MouseFilter = MouseFilter.Stop };
        var inner = new Control { Size = new Vector2(50, 50) };
        panel.AddChild(inner);
        root.AddChild(panel);
        root.DoLayout(TestViewport);

        var hit = root.HitTest(new Vector2(25, 25));
        hit.Should().Be(panel);
    }

    [Fact]
    public void Route_Handled_StopsBubbling()
    {
        var root = new TestControlRoot();
        var child = new TestControlThatHandles { Size = new Vector2(50, 50) };
        root.AddChild(child);
        root.DoLayout(TestViewport);

        var args = new MouseButtonEventArgs { Position = new Vector2(25, 25), Button = MouseButton.Left, Pressed = true };
        root.RouteMouseButtonDownPublic(child, args);

        args.Handled.Should().BeTrue();
        root.BubblingReceived.Should().BeFalse();
    }

    [Fact]
    public void Route_PreviewRunsBeforeBubble_TunnelThenBubbleOrder()
    {
        var root = new TestControlTunnelOrder { Size = new Vector2(100, 100) };
        var child = new Control { Size = new Vector2(50, 50) };
        root.AddChild(child);
        root.DoLayout(TestViewport);

        var args = new MouseButtonEventArgs { Position = new Vector2(25, 25), Button = MouseButton.Left, Pressed = true };
        root.RouteMouseButtonDownPublic(child, args);

        root.OrderLog.Should().ContainInOrder("Preview", "Bubble");
    }
}

internal class TestControlRoot : Control
{
    public bool BubblingReceived { get; private set; }
    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        BubblingReceived = true;
    }
    public void RouteMouseButtonDownPublic(Control? hitTarget, MouseButtonEventArgs args) =>
        typeof(Control).GetMethod("RouteMouseButtonDown", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(this, new object?[] { hitTarget, args });
}

internal class TestControlThatHandles : Control
{
    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}

internal class TestControlTunnelOrder : Control
{
    private readonly List<string> _orderLog = new();
    public IReadOnlyList<string> OrderLog => _orderLog;

    public override void OnPreviewMouseButtonDown(MouseButtonEventArgs e)
    {
        _orderLog.Add("Preview");
    }

    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        _orderLog.Add("Bubble");
    }

    public void RouteMouseButtonDownPublic(Control? hitTarget, MouseButtonEventArgs args) =>
        typeof(Control).GetMethod("RouteMouseButtonDown", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(this, new object?[] { hitTarget, args });
}
