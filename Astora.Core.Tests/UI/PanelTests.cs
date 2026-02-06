using Astora.Core.UI;
using Astora.Core.UI.Rendering;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class PanelTests
{
    [Fact]
    public void ComputeDesiredSize_ReturnsSize_LikeControl()
    {
        var panel = new Panel { Size = new Vector2(100, 50) };
        var desired = panel.ComputeDesiredSize();
        desired.Should().Be(new Vector2(100, 50));
        panel.DesiredSize.Should().Be(desired);
    }

    [Fact]
    public void ArrangeChildren_SetsFinalRect()
    {
        var panel = new Panel();
        var rect = new Rectangle(10, 20, 80, 40);
        panel.ArrangeChildren(rect);
        panel.FinalRect.Should().Be(rect);
    }

    [Fact]
    public void Draw_WhenUIDrawContextCurrentNull_DoesNotThrow()
    {
        UIDrawContext.SetCurrent(null);
        var panel = new Panel { Size = new Vector2(50, 50) };
        panel.ArrangeChildren(new Rectangle(0, 0, 50, 50));
        panel.Invoking(p => p.Draw(null!)).Should().NotThrow();
    }

    [Fact]
    public void Draw_WhenVisibleFalse_DoesNotUseBatcher()
    {
        var panel = new Panel { Visible = false, Size = new Vector2(10, 10) };
        panel.ArrangeChildren(new Rectangle(0, 0, 10, 10));
        panel.Invoking(p => p.Draw(null!)).Should().NotThrow();
    }
}
