using Astora.Core.UI;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.UI;

public class ThemeTests
{
    [Fact]
    public void Theme_SetColor_GetColor_ReturnsValue()
    {
        var theme = new Theme();
        var color = new Color(100, 150, 200, 255);
        theme.SetColor("panel_bg", color);

        theme.GetColor("panel_bg", out var result).Should().BeTrue();
        result.Should().Be(color);
    }

    [Fact]
    public void Theme_GetColor_UnknownKey_ReturnsFalse()
    {
        var theme = new Theme();
        theme.GetColor("unknown", out _).Should().BeFalse();
    }

    [Fact]
    public void Theme_SetConstant_GetConstant_ReturnsValue()
    {
        var theme = new Theme();
        theme.SetConstant("spacing", 12);

        theme.GetConstant("spacing", out var result).Should().BeTrue();
        result.Should().Be(12);
    }

    [Fact]
    public void Theme_GetConstant_UnknownKey_ReturnsFalse()
    {
        var theme = new Theme();
        theme.GetConstant("unknown", out _).Should().BeFalse();
    }

    [Fact]
    public void Control_GetThemeColor_NoThemeOrOverride_ReturnsDefault()
    {
        var c = new Control();
        c.GetThemeColor("any", Color.Red).Should().Be(Color.Red);
    }

    [Fact]
    public void Control_GetThemeColor_WithOverride_ReturnsOverride()
    {
        var c = new Control();
        var over = new Color(1, 2, 3, 4);
        c.AddThemeColorOverride("key", over);
        c.GetThemeColor("key", Color.White).Should().Be(over);
    }

    [Fact]
    public void Control_GetThemeColor_WithThemeOnRoot_ReturnsThemeValue()
    {
        var root = new Control();
        var theme = new Theme();
        theme.SetColor("bg", new Color(10, 20, 30, 255));
        root.Theme = theme;

        root.GetThemeColor("bg", Color.White).Should().Be(new Color(10, 20, 30, 255));
    }

    [Fact]
    public void Control_GetThemeColor_ChildInheritsFromParentTheme()
    {
        var root = new Control();
        var theme = new Theme();
        theme.SetColor("fg", new Color(40, 50, 60, 255));
        root.Theme = theme;
        var child = new Control();
        root.AddChild(child);

        child.GetThemeColor("fg", Color.White).Should().Be(new Color(40, 50, 60, 255));
    }

    [Fact]
    public void Control_GetThemeColor_OverrideTakesPrecedenceOverParentTheme()
    {
        var root = new Control();
        var theme = new Theme();
        theme.SetColor("clr", new Color(1, 1, 1, 255));
        root.Theme = theme;
        var child = new Control();
        child.AddThemeColorOverride("clr", new Color(2, 2, 2, 255));
        root.AddChild(child);

        child.GetThemeColor("clr", Color.White).Should().Be(new Color(2, 2, 2, 255));
    }
}
