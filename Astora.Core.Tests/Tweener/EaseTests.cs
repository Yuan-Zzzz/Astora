using Astora.Core.Tweener;
using FluentAssertions;
using System.Reflection;

namespace Astora.Core.Tests.Tweener;

public class EaseTests
{
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Collects all public static Func fields from the Ease class
    /// </summary>
    private static IEnumerable<Func<float, float>> GetAllEaseFunctions()
    {
        return typeof(Ease)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Func<float, float>))
            .Select(f => (Func<float, float>)f.GetValue(null)!);
    }

    private static IEnumerable<object[]> GetAllEaseFunctionData()
    {
        return typeof(Ease)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Func<float, float>))
            .Select(f => new object[] { f.Name, (Func<float, float>)f.GetValue(null)! });
    }

    [Fact]
    public void Linear_ReturnsInputValue()
    {
        Ease.Linear(0f).Should().BeApproximately(0f, Epsilon);
        Ease.Linear(0.5f).Should().BeApproximately(0.5f, Epsilon);
        Ease.Linear(1f).Should().BeApproximately(1f, Epsilon);
    }

    [Theory]
    [MemberData(nameof(AllEaseFunctions))]
    public void AllEaseFunctions_AtZero_ReturnZero(string name, Func<float, float> ease)
    {
        ease(0f).Should().BeApproximately(0f, Epsilon, $"Ease.{name}(0) should be 0");
    }

    [Theory]
    [MemberData(nameof(AllEaseFunctions))]
    public void AllEaseFunctions_AtOne_ReturnOne(string name, Func<float, float> ease)
    {
        ease(1f).Should().BeApproximately(1f, Epsilon, $"Ease.{name}(1) should be 1");
    }

    [Fact]
    public void InOutQuad_AtHalf_ReturnsHalf()
    {
        Ease.InOutQuad(0.5f).Should().BeApproximately(0.5f, Epsilon);
    }

    [Fact]
    public void InQuad_IsMonotonicallyIncreasing()
    {
        float prev = 0f;
        for (float t = 0.1f; t <= 1f; t += 0.1f)
        {
            var val = Ease.InQuad(t);
            val.Should().BeGreaterThanOrEqualTo(prev);
            prev = val;
        }
    }

    [Fact]
    public void OutBounce_AtHalf_IsGreaterThanHalf()
    {
        // OutBounce overshoots around the middle
        Ease.OutBounce(0.5f).Should().BeGreaterThan(0.5f);
    }

    public static IEnumerable<object[]> AllEaseFunctions()
    {
        return GetAllEaseFunctionData();
    }
}
