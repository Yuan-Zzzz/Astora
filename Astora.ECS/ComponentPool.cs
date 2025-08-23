namespace Astora.ECS;

public interface IComponentPool
{
    void RemoveIfContains(int entityId);
}

public sealed class ComponentPool<T> : IComponentPool
{
    private readonly SparseSets _set;
    private readonly T[] _componentInstances;

    public ComponentPool(int maxComponets)
    {
        _set = new SparseSets(maxComponets);
        _componentInstances = new T[maxComponets];
    }

    public void Add(int entityId, T value)
    {
        _set.Add(entityId);
        _componentInstances[_set.IndexOf(entityId)] = value;
    }

    public ref T Get(int entityId) => ref _componentInstances[_set.IndexOf(entityId)];

    public bool Contains(int entityId) => _set.Contains(entityId);
    
    public void RemoveIfContains(int entityId)
    {
        if (Contains(entityId)) Remove(entityId);
    }
    private void Remove(int entityId) => _set.Remove(entityId);
}