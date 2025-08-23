namespace Astora.Core.Time;

public interface ITime
{
    float Delta { get; }
    float Total { get; }
}

// <summary>不可变的时间快照实现。</summary>
public readonly record struct Time(float Delta, float Total) : ITime;