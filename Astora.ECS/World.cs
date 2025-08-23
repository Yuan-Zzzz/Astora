namespace Astora.ECS;
using Entity = Int32;
public class World
{
    private readonly int _PageSize;
    private Dictionary<Type, IComponentPool> _componentPools = new();
    private Entity nextEntity = 0;
    
    public World(int pageSize = 4096) => _PageSize = pageSize;

    /// <summary>
    /// Ensure that a component pool for type T exists, creating it if necessary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ComponentPool<T> Check<T>()
    {
        var type = typeof(T);
        if (_componentPools.TryGetValue(type, out var store)) return (ComponentPool<T>)_componentPools[type];
        
        var newStore = new ComponentPool<T>(_PageSize);
        _componentPools[type] = newStore;
        return newStore;
    } 
    
    /// <summary>
    /// Create a new entity.
    /// </summary>
    /// <returns></returns>
    public Entity Create() => nextEntity++;

    /// <summary>
    /// Destroy an entity and remove all its components.
    /// </summary>
    /// <param name="e"></param>
    public void Destroy(Entity e)
    {
        foreach (var pool in _componentPools.Values)
        {
            pool.RemoveIfContains(e);
        }
    }
    
    /// <summary>
    /// Add a component of type T to entity e.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="component"></param>
    /// <typeparam name="T"></typeparam>
    public void AddComponent<T>(Entity e, T component)
    {
        var store = Check<T>();
        store.Add(e, component);
    }
    
    /// <summary>
    /// Get a reference to the component of type T for entity e.
    /// </summary>
    /// <param name="e"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ref T GetComponent<T>(Entity e)
    {
        var store = Check<T>();
        return ref store.Get(e);
    }

    /// <summary>
    /// Try to get the component of type T for entity e. Returns true if found, false otherwise.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="component"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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
    
    /// <summary>
    /// Remove the component of type T from entity e, if it exists.
    /// </summary>
    /// <param name="e"></param>
    /// <typeparam name="T"></typeparam>
    public void RemoveComponent<T>(Entity e)
    {
        var store = Check<T>();
        store.RemoveIfContains(e);
    }
    
}

public static class WorldQueryExtensions
{
    public static Query<T> Query<T>(this World world) => new(world.Check<T>());

    public static Query<T1, T2> Query<T1, T2>(this World world) => new(world.Check<T1>(), world.Check<T2>());

    public static Query<T1, T2, T3> Query<T1, T2, T3>(this World world) =>
        new(world.Check<T1>(), world.Check<T2>(), world.Check<T3>());
}