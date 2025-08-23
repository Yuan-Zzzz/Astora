namespace Astora.ECS;
using Entity = Int32;
public class World
{
    private readonly int _maxEntities;
    private Dictionary<Type, IComponentPool> _componentPools = new();
    private Entity nextEntity = 0;
    
    public World(int maxEntities) => _maxEntities = maxEntities;

    /// <summary>
    /// Ensure that a component pool for type T exists, creating it if necessary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ComponentPool<T> Check<T>()
    {
        var type = typeof(T);
        if (_componentPools.TryGetValue(type, out var store)) return (ComponentPool<T>)_componentPools[type];
        
        var newStore = new ComponentPool<T>(_maxEntities);
        _componentPools[type] = newStore;
        return newStore;
    } 
    
    public Entity Create() => nextEntity++;

    public void Destroy(Entity e)
    {
        foreach (var pool in _componentPools.Values)
        {
            pool.RemoveIfContains(e);
        }
    }
    
    public void AddComponent<T>(Entity e, T component)
    {
        var store = Check<T>();
        store.Add(e, component);
    }
    
    public ref T GetComponent<T>(Entity e)
    {
        var store = Check<T>();
        return ref store.Get(e);
    }

    public bool TryGetComponent<T>(Entity e, ref T component)
    {
        var store = Check<T>();
        if (store.Contains(e))
        {
            component = store.Get(e);
            return true;
        }

        return false;
    }
    
    public void RemoveComponent<T>(Entity e)
    {
        var store = Check<T>();
        store.RemoveIfContains(e);
    }
    
}