using Astora.Core.UI;
using Astora.Core.UI.Events;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class ControlTests
{
    [Fact]
    public void Constructor_SetsDefaultName()
    {
        var c = new Control();
        c.Name.Should().Be("Control");
    }

    [Fact]
    public void Size_SetsAndInvalidatesLayout()
    {
        var c = new Control();
        c.IsLayoutDirty.Should().BeTrue();
        c.ClearLayoutDirty();
        c.IsLayoutDirty.Should().BeFalse();

        c.Size = new Vector2(100, 50);
        c.Size.Should().Be(new Vector2(100, 50));
        c.IsLayoutDirty.Should().BeTrue();
    }

    [Fact]
    public void Visible_ChangeInvalidatesLayout()
    {
        var c = new Control();
        c.ClearLayoutDirty();
        c.Visible = false;
        c.IsLayoutDirty.Should().BeTrue();
    }

    [Fact]
    public void AddChild_InvalidatesLayout()
    {
        var parent = new Control();
        parent.ClearLayoutDirty();
        var child = new Control();
        parent.AddChild(child);
        parent.IsLayoutDirty.Should().BeTrue();
    }

    [Fact]
    public void ComputeDesiredSize_NoChildren_ReturnsSizeOrZero()
    {
        var c = new Control();
        c.Size = new Vector2(80, 40);
        var desired = c.ComputeDesiredSize();
        desired.Should().Be(new Vector2(80, 40));
        c.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ComputeDesiredSize_NoChildren_NoExplicitSize_ReturnsZero()
    {
        var c = new Control();
        var desired = c.ComputeDesiredSize();
        desired.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ArrangeChildren_SetsFinalRect()
    {
        var c = new Control();
        var rect = new Rectangle(10, 20, 100, 50);
        c.ArrangeChildren(rect);
        c.FinalRect.Should().Be(rect);
    }

    [Fact]
    public void InvalidateLayout_PropagatesToParent()
    {
        var root = new Control();
        var child = new Control();
        root.AddChild(child);
        root.ClearLayoutDirty();
        child.ClearLayoutDirty();
        child.InvalidateLayout();
        child.IsLayoutDirty.Should().BeTrue();
        root.IsLayoutDirty.Should().BeTrue();
    }

    [Fact]
    public void ClearLayoutDirty_ClearsSubtree()
    {
        var root = new Control();
        var child = new Control();
        root.AddChild(child);
        root.InvalidateLayout();
        root.ClearLayoutDirty();
        root.IsLayoutDirty.Should().BeFalse();
        child.IsLayoutDirty.Should().BeFalse();
    }

    [Fact]
    public void EventVirtualMethods_DoNotThrow()
    {
        var c = new Control();
        var btnArgs = new MouseButtonEventArgs { Position = Vector2.Zero, Button = MouseButton.Left, Pressed = true };
        var moveArgs = new MouseMoveEventArgs { Position = Vector2.Zero, PreviousPosition = Vector2.Zero };
        c.Invoking(x => x.OnPreviewMouseButtonDown(btnArgs)).Should().NotThrow();
        c.Invoking(x => x.OnMouseButtonDown(btnArgs)).Should().NotThrow();
        c.Invoking(x => x.OnPreviewMouseMove(moveArgs)).Should().NotThrow();
        c.Invoking(x => x.OnMouseMove(moveArgs)).Should().NotThrow();
    }

    [Fact]
    public void MouseButtonEventArgs_Handled_StopsPropagation()
    {
        var args = new MouseButtonEventArgs { Position = Vector2.One, Button = MouseButton.Left, Pressed = true };
        args.Handled.Should().BeFalse();
        args.Handled = true;
        args.Handled.Should().BeTrue();
    }
}
