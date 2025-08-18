namespace Astora.Engine.Core;

public interface ITime
{
    float Delta { get; }
    float Total { get; }
}

public sealed class Time(float delta, float total) : ITime
{
    public float Delta { get; } = delta;
    public float Total { get; } = total;
}