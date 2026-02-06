using Astora.Core.UI;
using Astora.Core.UI.Container;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class BoxContainerTests
{
    [Fact]
    public void ComputeDesiredSize_Vertical_SumHeightsAndMaxWidth()
    {
        var box = new BoxContainer { Vertical = true, Spacing = 2 };
        var a = new Control { Size = new Vector2(60, 20) };
        var b = new Control { Size = new Vector2(100, 30) };
        box.AddChild(a);
        box.AddChild(b);

        a.ComputeDesiredSize();
        b.ComputeDesiredSize();
        var desired = box.ComputeDesiredSize();

        desired.X.Should().Be(100);
        desired.Y.Should().Be(20 + 2 + 30);
        box.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ComputeDesiredSize_Horizontal_SumWidthsAndMaxHeight()
    {
        var box = new BoxContainer { Vertical = false, Spacing = 4 };
        var a = new Control { Size = new Vector2(40, 50) };
        var b = new Control { Size = new Vector2(60, 30) };
        box.AddChild(a);
        box.AddChild(b);

        a.ComputeDesiredSize();
        b.ComputeDesiredSize();
        var desired = box.ComputeDesiredSize();

        desired.X.Should().Be(40 + 4 + 60);
        desired.Y.Should().Be(50);
        box.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ArrangeChildren_Vertical_StacksChildren()
    {
        var box = new BoxContainer { Vertical = true, Spacing = 2 };
        var a = new Control { Size = new Vector2(60, 20) };
        var b = new Control { Size = new Vector2(100, 30) };
        box.AddChild(a);
        box.AddChild(b);
        a.ComputeDesiredSize();
        b.ComputeDesiredSize();
        box.ComputeDesiredSize();

        var rect = new Rectangle(10, 5, 200, 100);
        box.ArrangeChildren(rect);

        box.FinalRect.Should().Be(rect);
        a.FinalRect.Should().Be(new Rectangle(10, 5, 200, 20));
        b.FinalRect.Should().Be(new Rectangle(10, 5 + 20 + 2, 200, 30));
    }

    [Fact]
    public void ArrangeChildren_Horizontal_PlacesChildrenSideBySide()
    {
        var box = new BoxContainer { Vertical = false, Spacing = 4 };
        var a = new Control { Size = new Vector2(40, 50) };
        var b = new Control { Size = new Vector2(60, 30) };
        box.AddChild(a);
        box.AddChild(b);
        a.ComputeDesiredSize();
        b.ComputeDesiredSize();
        box.ComputeDesiredSize();

        var rect = new Rectangle(0, 0, 200, 80);
        box.ArrangeChildren(rect);

        a.FinalRect.Should().Be(new Rectangle(0, 0, 40, 80));
        b.FinalRect.Should().Be(new Rectangle(40 + 4, 0, 60, 80));
    }

    [Fact]
    public void EmptyBox_DesiredSizeZero()
    {
        var box = new BoxContainer();
        box.ComputeDesiredSize().Should().Be(Vector2.Zero);
        box.DesiredSize.Should().Be(Vector2.Zero);
    }
}
