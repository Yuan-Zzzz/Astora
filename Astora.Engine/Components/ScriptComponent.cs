using Astora.Core.Time;

namespace Astora.Engine.Components;

public sealed class ScriptComponent
{
    public Behaviour? Instance;
    public Func<Behaviour>? Instantiate;
    public Action<ScriptComponent>? Destroy;

    public ScriptComponent(Func<Behaviour> factory)
    {
        Instantiate = factory;
        Destroy = nsc => nsc.Instance?.OnDestroy();
    }
}

public abstract class Behaviour
{
    public Astora.Engine.Entity Entity { get; internal set; }
    internal ECS.World? _world; // 引擎内部注入

    // 组件访问（已有）
    protected ref T GetComponent<T>() => ref _world!.GetComponent<T>(Entity.Id);
    protected ref T GetComponentOf<T>(Astora.Engine.Entity e) => ref _world!.GetComponent<T>(e.Id);

    // === 新增便捷方法 ===
    protected ref T AddComponent<T>(T component)
    {
        _world!.AddComponent(Entity.Id, component);
        return ref _world!.GetComponent<T>(Entity.Id);
    }
    protected bool HasComponent<T>() => _world!.Check<T>().Contains(Entity.Id);
    protected void RemoveComponent<T>() => _world!.RemoveComponent<T>(Entity.Id);
    protected bool TryGetComponent<T>(out T component)
    {
        component = default;
        return _world!.TryGetComponent(Entity.Id, ref component);
    }
    protected ref T GetOrAddComponent<T>() where T : new()
    {
        if (!HasComponent<T>())
            _world!.AddComponent(Entity.Id, new T());
        return ref _world!.GetComponent<T>(Entity.Id);
    }

    public virtual void OnCreate() { }
    public virtual void OnUpdate(ITime t) { }
    public virtual void OnDestroy() { }
}