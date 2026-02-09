using System.Collections.Concurrent;

namespace Astora.Editor.Core.Events;

/// <summary>
/// 线程安全的轻量事件总线（无反射、无 DI 依赖）。
/// </summary>
public sealed class EventBus : IEventBus
{
    private sealed class Subscription : IDisposable
    {
        private readonly Action _dispose;
        private int _disposed;

        public Subscription(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;
            _dispose();
        }
    }

    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly object _gate = new();

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        lock (_gate)
        {
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }

        return new Subscription(() =>
        {
            lock (_gate)
            {
                if (_handlers.TryGetValue(type, out var list))
                {
                    list.Remove(handler);
                    if (list.Count == 0)
                        _handlers.TryRemove(type, out _);
                }
            }
        });
    }

    public void Publish<TEvent>(TEvent evt)
    {
        List<Delegate>? snapshot = null;
        var type = typeof(TEvent);
        lock (_gate)
        {
            if (_handlers.TryGetValue(type, out var list) && list.Count > 0)
                snapshot = new List<Delegate>(list);
        }

        if (snapshot == null)
            return;

        foreach (var d in snapshot)
        {
            if (d is Action<TEvent> h)
                h(evt);
        }
    }
}

