using System.Collections.Concurrent;

namespace Astora.Core.Event;
public sealed class EventBus : IEventBus
{
    private readonly object _gate = new();
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public IDisposable Subscribe<T>(Action<T> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));

        var list = _handlers.GetOrAdd(typeof(T), _ => new List<Delegate>());
        lock (_gate)
        {
            list.Add(handler);
        }

        return new Unsubscriber(() =>
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
            {
                lock (_gate)
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                        _handlers.TryRemove(typeof(T), out _);
                }
            }
        });
    }

    public void Publish<T>(T @event)
    {
        if (!_handlers.TryGetValue(typeof(T), out var list)) return;

        Delegate[] snapshot;
        lock (_gate)
            snapshot = list.ToArray();

        foreach (var d in snapshot)
        {
            try { ((Action<T>)d).Invoke(@event); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }

    public void Clear() => _handlers.Clear();

    private sealed class Unsubscriber : IDisposable
    {
        private readonly Action _dispose;
        private bool _disposed;
        public Unsubscriber(Action dispose) => _dispose = dispose;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _dispose();
        }
    }
}