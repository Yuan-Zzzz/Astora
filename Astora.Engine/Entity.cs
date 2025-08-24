using Astora.ECS;
using Astora.Engine.Components;

namespace Astora.Engine;

public readonly struct Entity
{
    public int Id { get; }
    internal World World { get; }

    internal Entity(int id, World world)
    {
        Id = id;
        World = world;
    }

    public bool IsValid => World != null;
    
    public ref T AddComponent<T>(T component)
    {
        World.AddComponent(Id, component);
        return ref World.GetComponent<T>(Id);
    }

    public bool HasComponent<T>() => World.Check<T>().Contains(Id);

    public ref T GetComponent<T>() => ref World.GetComponent<T>(Id);

    public bool TryGetComponent<T>(out T component)
    {
        component = default;
        return World.TryGetComponent(Id, ref component);
    }

    public void RemoveComponent<T>() => World.RemoveComponent<T>(Id);

    // Tag 快捷访问
    public string? Tag
    {
        get
        {
            if (TryGetComponent<TagComponent>(out var tag)) return tag.Tag;
            return null;
        }
        set
        {
            if (value == null) return;
            if (HasComponent<TagComponent>())
            {
                ref var tag = ref GetComponent<TagComponent>();
                tag.Tag = value;
            }
            else
            {
                AddComponent(new TagComponent(value));
            }
        }
    }

    public override string ToString() => Tag ?? $"Entity({Id})";

    // 相等性（同一个 World & Id）
    public override int GetHashCode() => HashCode.Combine(Id, World);
    public override bool Equals(object? obj) => obj is Entity other && other.Id == Id && other.World == World;
    public static bool operator ==(Entity a, Entity b) => a.Equals(b);
    public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
}

public static class EntityExtensions
{
    /// <summary>
    /// 快捷添加或获取组件（若存在就返回已存在的引用）。
    /// </summary>
    public static ref T GetOrAddComponent<T>(this Entity e) where T : new()
    {
        if (!e.HasComponent<T>())
        {
            e.AddComponent(new T());
        }
        return ref e.GetComponent<T>();
    }
}