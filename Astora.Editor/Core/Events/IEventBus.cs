namespace Astora.Editor.Core.Events;

/// <summary>
/// 简单事件总线：用于在 UI/服务/工具之间解耦通信。
/// </summary>
public interface IEventBus
{
    IDisposable Subscribe<TEvent>(Action<TEvent> handler);
    void Publish<TEvent>(TEvent evt);
}

