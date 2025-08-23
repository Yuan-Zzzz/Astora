namespace Astora.ECS;

public interface IComponentPool
{
    void RemoveIfContains(int entityId);
}

public sealed class ComponentPool<T> : IComponentPool
{
    public readonly SparseSets Set;
    private readonly T[] _componentInstances;

    public ComponentPool(int maxComponets)
    {
        Set = new SparseSets(maxComponets);
        _componentInstances = new T[maxComponets];
    }

    public void Add(int entityId, T value)
    {
        Set.Add(entityId);
        _componentInstances[Set.IndexOf(entityId)] = value;
    }

    public ref T Get(int entityId) => ref _componentInstances[Set.IndexOf(entityId)];

    public bool Contains(int entityId) => Set.Contains(entityId);
    
    public void RemoveIfContains(int entityId)
    {
        if (Contains(entityId)) Remove(entityId);
    }
    private void Remove(int entityId) => Set.Remove(entityId);
}