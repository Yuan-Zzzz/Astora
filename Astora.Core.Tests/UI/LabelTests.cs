using Astora.Core.UI;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

/// <summary>
/// Label tests without a real font (no GraphicsDevice).
/// Covers layout, property defaults, invalidation and draw-early-exit paths.
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
    public void ComputeDesiredSize_NoFontResourceNoSize_ReturnsZero()
    {
        var label = new Label();
        var desired = label.ComputeDesiredSize();
        desired.Should().Be(Vector2.Zero);
        label.DesiredSize.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ComputeDesiredSize_NoFontResourceWithText_ReturnsZero()
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

    [Fact]
    public void FontSize_DefaultIs16()
    {
        var label = new Label();
        label.FontSize.Should().Be(16f);
    }

    [Fact]
    public void FontResource_DefaultIsNull()
    {
        var label = new Label();
        label.FontResource.Should().BeNull();
    }

    [Fact]
    public void FontSize_Set_InvalidatesLayout()
    {
        var root = new Control();
        var label = new Label { Size = new Vector2(50, 20) };
        root.AddChild(label);
        root.DoLayout(new Rectangle(0, 0, 200, 200));
        root.IsLayoutDirty.Should().BeFalse();

        label.FontSize = 32f;
        root.IsLayoutDirty.Should().BeTrue();
    }

    [Fact]
    public void FontSize_SetSameValue_DoesNotInvalidate()
    {
        var root = new Control();
        var label = new Label { Size = new Vector2(50, 20) };
        root.AddChild(label);
        root.DoLayout(new Rectangle(0, 0, 200, 200));
        root.IsLayoutDirty.Should().BeFalse();

        label.FontSize = 16f; // default value, no change
        root.IsLayoutDirty.Should().BeFalse();
    }

    [Fact]
    public void Text_Set_InvalidatesLayout()
    {
        var root = new Control();
        var label = new Label { Size = new Vector2(50, 20) };
        root.AddChild(label);
        root.DoLayout(new Rectangle(0, 0, 200, 200));
        root.IsLayoutDirty.Should().BeFalse();

        label.Text = "changed";
        root.IsLayoutDirty.Should().BeTrue();
    }

    [Fact]
    public void Draw_NoFontResource_DoesNotThrow()
    {
        var label = new Label { Text = "Hello" };
        label.ArrangeChildren(new Rectangle(0, 0, 100, 30));
        label.Invoking(l => l.Draw(null!)).Should().NotThrow();
    }

    [Fact]
    public void Draw_NotVisible_DoesNotThrow()
    {
        var label = new Label { Visible = false, Text = "Hello" };
        label.ArrangeChildren(new Rectangle(0, 0, 100, 30));
        label.Invoking(l => l.Draw(null!)).Should().NotThrow();
    }

    [Fact]
    public void Draw_EmptyText_DoesNotThrow()
    {
        var label = new Label { Text = "" };
        label.ArrangeChildren(new Rectangle(0, 0, 100, 30));
        label.Invoking(l => l.Draw(null!)).Should().NotThrow();
    }
}
