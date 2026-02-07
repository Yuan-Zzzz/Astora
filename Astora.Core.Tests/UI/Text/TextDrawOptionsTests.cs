using Astora.Core.UI.Text;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI.Text;

public class TextDrawOptionsTests
{
    [Fact]
    public void None_HasNoShadowOrOutline()
    {
        var opt = TextDrawOptions.None;
        opt.ShadowColor.Should().BeNull();
        opt.OutlineColor.Should().BeNull();
        opt.OutlineThickness.Should().Be(0);
    }

    [Fact]
    public void WithShadow_SetsOffsetAndColor()
    {
        var offset = new Vector2(2, 3);
        var color = new Color(0, 0, 0, 128);
        var opt = TextDrawOptions.WithShadow(offset, color);
        opt.ShadowOffset.Should().Be(offset);
        opt.ShadowColor.Should().Be(color);
        opt.OutlineColor.Should().BeNull();
    }

    [Fact]
    public void WithOutline_SetsColorAndThickness()
    {
        var color = Color.Black;
        var opt = TextDrawOptions.WithOutline(color, 2);
        opt.OutlineColor.Should().Be(color);
        opt.OutlineThickness.Should().Be(2);
        opt.ShadowColor.Should().BeNull();
    }
}
