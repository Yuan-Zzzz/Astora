using Astora.Core.Resources;
using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Astora.Core.Tests.Resources;

public class SpriteFramesTests
{
    // SpriteFrames requires a Texture2D which needs GraphicsDevice.
    // We test the data-structure logic without a real texture by passing null
    // and focusing on animation/frame management.

    [Fact]
    public void AddAnimation_CreatesNewAnimation()
    {
        var frames = new SpriteFrames(null!);
        frames.AddAnimation("idle", fps: 10, loop: true);

        frames.HasAnimation("idle").Should().BeTrue();
    }

    [Fact]
    public void HasAnimation_ReturnsFalseForNonExistent()
    {
        var frames = new SpriteFrames(null!);

        frames.HasAnimation("walk").Should().BeFalse();
    }

    [Fact]
    public void GetAnimation_ReturnsCorrectAnimation()
    {
        var frames = new SpriteFrames(null!);
        frames.AddAnimation("run", fps: 12, loop: false);

        var anim = frames.GetAnimation("run");

        anim.Should().NotBeNull();
        anim!.Name.Should().Be("run");
        anim.Fps.Should().Be(12f);
        anim.Loop.Should().BeFalse();
    }

    [Fact]
    public void GetAnimation_ReturnsNull_WhenNotFound()
    {
        var frames = new SpriteFrames(null!);

        var anim = frames.GetAnimation("missing");

        anim.Should().BeNull();
    }

    [Fact]
    public void AddFrame_AppendsToAnimation()
    {
        var frames = new SpriteFrames(null!);
        frames.AddAnimation("idle");

        var rect1 = new Rectangle(0, 0, 32, 32);
        var rect2 = new Rectangle(32, 0, 32, 32);
        frames.AddFrame("idle", rect1);
        frames.AddFrame("idle", rect2);

        var anim = frames.GetAnimation("idle");
        anim!.Frames.Should().HaveCount(2);
        anim.Frames[0].Should().Be(rect1);
        anim.Frames[1].Should().Be(rect2);
    }

    [Fact]
    public void AddFrame_ToNonExistentAnimation_DoesNotThrow()
    {
        var frames = new SpriteFrames(null!);

        var act = () => frames.AddFrame("missing", new Rectangle(0, 0, 32, 32));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddAnimation_DuplicateName_DoesNotOverwrite()
    {
        var frames = new SpriteFrames(null!);
        frames.AddAnimation("idle", fps: 10, loop: true);
        frames.AddFrame("idle", new Rectangle(0, 0, 32, 32));

        // Try adding again with different params
        frames.AddAnimation("idle", fps: 20, loop: false);

        var anim = frames.GetAnimation("idle");
        anim!.Fps.Should().Be(10f); // Original fps preserved
        anim.Frames.Should().HaveCount(1); // Frames preserved
    }
}
