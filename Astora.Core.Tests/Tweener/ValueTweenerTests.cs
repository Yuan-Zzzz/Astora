using Astora.Core.Tweener;
using FluentAssertions;

namespace Astora.Core.Tests.Tweener;

public class ValueTweenerTests
{
    [Fact]
    public void Update_CompletesAfterDuration()
    {
        bool completed = false;
        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .OnComplete(() => completed = true)
            .Build();

        // Should not be done after 0.5s
        tween.Update(0.5f).Should().BeFalse();
        completed.Should().BeFalse();

        // Should be done after another 0.5s (total 1.0s)
        tween.Update(0.5f).Should().BeTrue();
        completed.Should().BeTrue();
    }

    [Fact]
    public void Update_CallsOnUpdateWithInterpolatedValue()
    {
        float lastValue = -1f;
        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .OnUpdate(v => lastValue = v)
            .Build();

        tween.Update(0.5f);

        lastValue.Should().BeApproximately(50f, 0.01f);
    }

    [Fact]
    public void Update_ReachesExactEndValue()
    {
        float lastValue = -1f;
        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .OnUpdate(v => lastValue = v)
            .Build();

        tween.Update(1.0f);

        lastValue.Should().BeApproximately(100f, 0.01f);
    }

    [Fact]
    public void Update_WithEase_AppliesEasingFunction()
    {
        float lastValue = -1f;
        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .SetEase(Ease.InQuad)
            .OnUpdate(v => lastValue = v)
            .Build();

        tween.Update(0.5f);

        // InQuad at t=0.5 gives 0.25, so value should be 25
        lastValue.Should().BeApproximately(25f, 0.01f);
    }

    [Fact]
    public void Create_ZeroDuration_ThrowsArgumentException()
    {
        var act = () => ValueTweener<float>.Create(0f, 100f, 0f).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_AlreadyCompleted_ReturnsTrue()
    {
        var tween = ValueTweener<float>.Create(0f, 100f, 0.5f).Build();

        tween.Update(1.0f); // completes
        tween.Update(0.1f).Should().BeTrue(); // should still report completed
    }

    [Fact]
    public void Start_RegistersWithTweenCore()
    {
        TweenCore.Clear();

        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .Start();

        // After calling Start, TweenCore should drive it
        // We verify by updating TweenCore and seeing the tween progress
        float lastValue = -1f;
        var trackedTween = ValueTweener<float>.Create(0f, 50f, 1.0f)
            .OnUpdate(v => lastValue = v)
            .Start();

        TweenCore.Update(0.5f);

        lastValue.Should().BeApproximately(25f, 0.5f);

        TweenCore.Clear();
    }
}
