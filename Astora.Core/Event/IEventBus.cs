namespace Astora.Core.Event;

public interface IEventBus
{
    IDisposable Subscribe<T>(Action<T> handler);
    
    void Publish<T>(T @event);
    
    void Clear();
}