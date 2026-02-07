using Astora.Core.UI.Text;
using FluentAssertions;

namespace Astora.Core.Tests.UI.Text;

public class BBCodeParserTests
{
    [Fact]
    public void ToRichTextCommands_Empty_ReturnsEmpty()
    {
        BBCodeParser.ToRichTextCommands("").Should().Be("");
        BBCodeParser.ToRichTextCommands(null!).Should().BeNull();
    }

    [Fact]
    public void ToRichTextCommands_PlainText_Unchanged()
    {
        BBCodeParser.ToRichTextCommands("Hello").Should().Be("Hello");
    }

    [Fact]
    public void ToRichTextCommands_ColorTag_ConvertsToFSS()
    {
        var result = BBCodeParser.ToRichTextCommands("[color=red]hi[/color]");
        result.Should().Be("/c[red]hi/cd");
    }

    [Fact]
    public void ToRichTextCommands_ColorHex_ConvertsToFSS()
    {
        var result = BBCodeParser.ToRichTextCommands("[color=#ff0000]R[/color]");
        result.Should().Be("/c[#ff0000]R/cd");
    }

    [Fact]
    public void ToRichTextCommands_BoldTag_ConvertsToStroke()
    {
        var result = BBCodeParser.ToRichTextCommands("[b]bold[/b]");
        result.Should().Be("/esbold/ed");
    }

    [Fact]
    public void ToRichTextCommands_LineBreak_ConvertsToN()
    {
        BBCodeParser.ToRichTextCommands("[n]").Should().Be("/n");
        BBCodeParser.ToRichTextCommands("[br]").Should().Be("/n");
    }

    [Fact]
    public void ToRichTextCommands_WithTime_WaveProducesVCommand()
    {
        var result = BBCodeParser.ToRichTextCommands("[wave]w[/wave]", 0f);
        result.Should().Contain("/v[");
        result.Should().Contain("w");
        result.Should().EndWith("/vd");
    }

    [Fact]
    public void ToRichTextCommands_WithTime_RainbowProducesColor()
    {
        var result = BBCodeParser.ToRichTextCommands("[rainbow]r[/rainbow]", 0.5f);
        result.Should().Contain("/c[#");
        result.Should().Contain("r");
        result.Should().EndWith("/cd");
    }
}
