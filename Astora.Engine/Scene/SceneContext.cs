using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Core;
using Astora.ECS; // 新增
using Astora.Engine; // 需要 Entity 包装

namespace Astora.Engine.Scene;

public readonly struct SceneContext
{
    public GraphicsDevice GraphicsDevice { get; }
    public LogicScheduler Logic { get; }
    public RenderScheduler Render { get; }
    public IServiceRegistry Services { get; }
    internal World World { get; } // 内部存取

    public SceneContext(GraphicsDevice gd, LogicScheduler logic, RenderScheduler render, IServiceRegistry services, World world)
    {
        GraphicsDevice = gd;
        Logic = logic;
        Render = render;
        Services = services;
        World = world;
    }

    public T Get<T>() where T : class => Services.Get<T>();

    // === 对外提供的实体与组件操作（封装 ECS） ===
    public Entity CreateEntity() => new Entity(World.Create(), World);
    public void DestroyEntity(Entity e) => World.Destroy(e.Id);

    public ref T AddComponent<T>(Entity e, T component) { World.AddComponent(e.Id, component); return ref World.GetComponent<T>(e.Id); }
    public bool HasComponent<T>(Entity e) => World.Check<T>().Contains(e.Id);
    public ref T GetComponent<T>(Entity e) => ref World.GetComponent<T>(e.Id);
    public bool TryGetComponent<T>(Entity e, out T component)
    {
        component = default;
        return World.TryGetComponent(e.Id, ref component);
    }
    public void RemoveComponent<T>(Entity e) => World.RemoveComponent<T>(e.Id);

    // 查询包装：返回 Entity 序列（简单 yield 封装）
    public IEnumerable<Entity> Query<T>()
    {
        foreach (var id in World.Query<T>())
            yield return new Entity(id, World);
    }
    public IEnumerable<Entity> Query<T1, T2>()
    {
        foreach (var id in World.Query<T1, T2>())
            yield return new Entity(id, World);
    }
    public IEnumerable<Entity> Query<T1, T2, T3>()
    {
        foreach (var id in World.Query<T1, T2, T3>())
            yield return new Entity(id, World);
    }
}