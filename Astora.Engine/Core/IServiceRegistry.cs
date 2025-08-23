namespace Astora.Engine.Core;

public interface IServiceRegistry
{
    void Add<T>(T instance) where T : class;
    T Get<T>() where T : class;
    bool TryGet<T>(out T? instance) where T : class;
}

public sealed class ServiceRegistry : IServiceRegistry
{
    private readonly Dictionary<Type, object> _map = new();
    public void Add<T>(T instance) where T : class => _map[typeof(T)] = instance!;
    public T Get<T>() where T : class => (T)_map[typeof(T)];
    public bool TryGet<T>(out T? instance) where T : class
    {
        if (_map.TryGetValue(typeof(T), out var obj)) { instance = (T)obj; return true; }
        instance = null; return false;
    }
}