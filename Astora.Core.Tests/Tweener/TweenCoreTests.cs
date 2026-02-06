using Astora.Core.Tweener;
using FluentAssertions;

namespace Astora.Core.Tests.Tweener;

public class TweenCoreTests : IDisposable
{
    public TweenCoreTests()
    {
        TweenCore.Clear();
    }

    public void Dispose()
    {
        TweenCore.Clear();
    }

    [Fact]
    public void Add_And_Update_DrivesTween()
    {
        float value = 0f;
        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .OnUpdate(v => value = v)
            .Build();

        TweenCore.Add(tween);
        TweenCore.Update(0.5f);

        value.Should().BeApproximately(50f, 0.1f);
    }

    [Fact]
    public void Update_RemovesCompletedTweens()
    {
        int completeCount = 0;
        var tween = ValueTweener<float>.Create(0f, 100f, 0.5f)
            .OnComplete(() => completeCount++)
            .Build();

        TweenCore.Add(tween);
        TweenCore.Update(1.0f); // Complete the tween

        completeCount.Should().Be(1);

        // Update again - should not cause issues (tween already removed)
        TweenCore.Update(0.1f);
        completeCount.Should().Be(1);
    }

    [Fact]
    public void Clear_RemovesAllTweens()
    {
        float value = 0f;
        var tween = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .OnUpdate(v => value = v)
            .Build();

        TweenCore.Add(tween);
        TweenCore.Clear();
        TweenCore.Update(0.5f);

        // Value should remain 0 because tween was cleared
        value.Should().Be(0f);
    }

    [Fact]
    public void MultipleTweens_AllDrivenConcurrently()
    {
        float value1 = 0f;
        float value2 = 0f;

        var tween1 = ValueTweener<float>.Create(0f, 100f, 1.0f)
            .OnUpdate(v => value1 = v)
            .Build();
        var tween2 = ValueTweener<float>.Create(0f, 200f, 1.0f)
            .OnUpdate(v => value2 = v)
            .Build();

        TweenCore.Add(tween1);
        TweenCore.Add(tween2);
        TweenCore.Update(0.5f);

        value1.Should().BeApproximately(50f, 0.1f);
        value2.Should().BeApproximately(100f, 0.1f);
    }
}
