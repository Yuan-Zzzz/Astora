using System;
using System.Runtime.CompilerServices;

namespace Astora.ECS;

public interface IComponentPool
{
    void RemoveIfContains(int entityId);
}

public sealed class ComponentPool<T> : IComponentPool
{
    public readonly SparseSets Set;
    private T[] _componentInstances;
    
    public ComponentPool(int pageSize = 4096)
    {
        Set = new SparseSets(pageSize);
        _componentInstances = new T[Math.Max(16, pageSize)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int needed)
    {
        if (needed <= _componentInstances.Length) return;
        int newLen = _componentInstances.Length;
        while (newLen < needed)
            newLen = newLen < 1024 ? newLen * 2 : newLen + newLen / 2;
        Array.Resize(ref _componentInstances, newLen);
    }

    public void Add(int entityId, T value)
    {
        Set.Add(entityId);
        int di = Set.IndexOf(entityId);
        EnsureCapacity(di + 1);
        _componentInstances[di] = value;
    }

    public ref T Get(int entityId) => ref _componentInstances[Set.IndexOf(entityId)];

    public bool Contains(int entityId) => Set.Contains(entityId);

    public void RemoveIfContains(int entityId)
    {
        int di = Set.IndexOf(entityId);
        if (di == SparseSets.Invalid) return;

        int last = Set.Count - 1;
        if (di != last)
        {
            _componentInstances[di] = _componentInstances[last];
        }

        Set.Remove(entityId);
    }
}