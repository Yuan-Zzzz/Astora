using Astora.Core.UI;
using Astora.Core.UI.Container;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class MarginContainerTests
{
    [Fact]
    public void ComputeDesiredSize_NoChild_ReturnsMarginSum()
    {
        var margin = new MarginContainer();
        margin.MarginLeft = 10;
        margin.MarginTop = 20;
        margin.MarginRight = 30;
        margin.MarginBottom = 40;

        var desired = margin.ComputeDesiredSize();

        desired.X.Should().Be(40);
        desired.Y.Should().Be(60);
        margin.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ComputeDesiredSize_SingleChild_ReturnsChildDesiredSizePlusMargins()
    {
        var margin = new MarginContainer();
        margin.SetMarginAll(8);
        var child = new Control { Size = new Vector2(100, 50) };
        margin.AddChild(child);
        child.ComputeDesiredSize();

        var desired = margin.ComputeDesiredSize();

        desired.X.Should().Be(100 + 8 + 8);
        desired.Y.Should().Be(50 + 8 + 8);
        margin.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ArrangeChildren_SingleChild_AssignsInnerRect()
    {
        var margin = new MarginContainer();
        margin.MarginLeft = 5;
        margin.MarginTop = 10;
        margin.MarginRight = 15;
        margin.MarginBottom = 20;
        var child = new Control { Size = new Vector2(80, 40) };
        margin.AddChild(child);
        child.ComputeDesiredSize();
        margin.ComputeDesiredSize();

        var rect = new Rectangle(0, 0, 200, 100);
        margin.ArrangeChildren(rect);

        margin.FinalRect.Should().Be(rect);
        child.FinalRect.Should().Be(new Rectangle(5, 10, 200 - 5 - 15, 100 - 10 - 20));
    }

    [Fact]
    public void ArrangeChildren_NoChild_OnlySetsOwnFinalRect()
    {
        var margin = new MarginContainer();
        margin.SetMarginAll(10);
        var rect = new Rectangle(20, 30, 100, 80);
        margin.ArrangeChildren(rect);
        margin.FinalRect.Should().Be(rect);
    }
}
