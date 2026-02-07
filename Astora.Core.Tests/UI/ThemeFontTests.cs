using Astora.Core.Resources;
using Astora.Core.UI;
using FluentAssertions;
using FontStashSharp;

namespace Astora.Core.Tests.UI;

/// <summary>
/// Tests for Theme font dictionary and Control.GetThemeFont resolution chain.
/// Uses a minimal FontResource stub (null FontSystem) since tests only verify
/// the resolution logic, not actual font rendering.
/// </summary>
public class ThemeFontTests
{
    /// <summary>Creates a lightweight FontResource for identity comparison (no real FontSystem).</summary>
    private static FontResource CreateStubFont(string tag = "stub")
    {
        return new FontResource(null!, tag);
    }

    #region Theme SetFont / GetFont

    [Fact]
    public void Theme_SetFont_GetFont_RoundTrips()
    {
        var theme = new Theme();
        var font = CreateStubFont();
        theme.SetFont("default_font", font);

        theme.GetFont("default_font", out var result).Should().BeTrue();
        result.Should().BeSameAs(font);
    }

    [Fact]
    public void Theme_GetFont_UnknownName_ReturnsFalse()
    {
        var theme = new Theme();
        theme.GetFont("missing", out _).Should().BeFalse();
    }

    [Fact]
    public void Theme_SetFont_OverwritesPrevious()
    {
        var theme = new Theme();
        var fontA = CreateStubFont("a");
        var fontB = CreateStubFont("b");
        theme.SetFont("default_font", fontA);
        theme.SetFont("default_font", fontB);

        theme.GetFont("default_font", out var result).Should().BeTrue();
        result.Should().BeSameAs(fontB);
    }

    #endregion

    #region Control.GetThemeFont resolution chain

    [Fact]
    public void GetThemeFont_NoThemeNoOverride_ReturnsNull()
    {
        var control = new Control();
        control.GetThemeFont("default_font").Should().BeNull();
    }

    [Fact]
    public void GetThemeFont_LocalOverride_ReturnsThat()
    {
        var control = new Control();
        var font = CreateStubFont();
        control.AddThemeFontOverride("default_font", font);

        control.GetThemeFont("default_font").Should().BeSameAs(font);
    }

    [Fact]
    public void GetThemeFont_InheritsFromParent()
    {
        var parent = new Control();
        var child = new Control();
        parent.AddChild(child);

        var font = CreateStubFont();
        parent.AddThemeFontOverride("default_font", font);

        child.GetThemeFont("default_font").Should().BeSameAs(font);
    }

    [Fact]
    public void GetThemeFont_LocalOverrideTakesPrecedenceOverParent()
    {
        var parent = new Control();
        var child = new Control();
        parent.AddChild(child);

        var parentFont = CreateStubFont("parent");
        var childFont = CreateStubFont("child");
        parent.AddThemeFontOverride("default_font", parentFont);
        child.AddThemeFontOverride("default_font", childFont);

        child.GetThemeFont("default_font").Should().BeSameAs(childFont);
    }

    [Fact]
    public void GetThemeFont_FallsBackToThemeResource()
    {
        var theme = new Theme();
        var font = CreateStubFont();
        theme.SetFont("default_font", font);

        var root = new Control { Theme = theme };
        var child = new Control();
        root.AddChild(child);

        // The resolution chain walks up to root, which has Theme set.
        // However, GetThemeFont checks parent chain first, then only root's _theme.
        // Root itself should resolve from its theme.
        root.GetThemeFont("default_font").Should().BeSameAs(font);
    }

    [Fact]
    public void GetThemeFont_DeepHierarchy_BubblesUp()
    {
        var grandparent = new Control();
        var parent = new Control();
        var child = new Control();
        grandparent.AddChild(parent);
        parent.AddChild(child);

        var font = CreateStubFont();
        grandparent.AddThemeFontOverride("default_font", font);

        child.GetThemeFont("default_font").Should().BeSameAs(font);
    }

    #endregion
}
