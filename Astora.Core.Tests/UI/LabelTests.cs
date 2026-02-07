using Astora.Core.UI;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

/// <summary>
/// Label tests without font (no font resource). Covers layout when Font is null.
/// </summary>
public class LabelTests
{
    [Fact]
    public void ComputeDesiredSize_ExplicitSize_ReturnsSize()
    {
        var label = new Label { Size = new Vector2(100, 24) };
        var desired = label.ComputeDesiredSize();
        desired.Should().Be(new Vector2(100, 24));
        label.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ComputeDesiredSize_NoFontNoSize_ReturnsZero()
    {
        var label = new Label();
        var desired = label.ComputeDesiredSize();
        desired.Should().Be(Vector2.Zero);
        label.DesiredSize.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ComputeDesiredSize_NoFontWithText_ReturnsZero()
    {
        var label = new Label { Text = "Hello" };
        var desired = label.ComputeDesiredSize();
        desired.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ArrangeChildren_SetsFinalRect()
    {
        var label = new Label();
        var rect = new Rectangle(10, 20, 80, 24);
        label.ArrangeChildren(rect);
        label.FinalRect.Should().Be(rect);
    }
}
