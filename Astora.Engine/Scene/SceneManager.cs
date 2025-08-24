using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Core;

namespace Astora.Engine.Scene;

/// <summary>
/// 只维护一个当前场景。Load 会：卸载旧 → 清空调度器 → 加载新 → 注册系统。
/// </summary>
public sealed class SceneManager
{
    private readonly IServiceRegistry _services;
    private readonly GraphicsDevice _gd;
    private readonly LogicScheduler _logic;
    private readonly RenderScheduler _render;

    public Scene? Current { get; private set; }

    public SceneManager(IServiceRegistry services,
        GraphicsDevice gd,
        LogicScheduler logic,
        RenderScheduler render)
    {
        _services = services;
        _gd = gd;
        _logic = logic;
        _render = render;
    }

    public void Load(Scene scene)
    {
        // 1) 卸载旧场景
        if (Current is not null && Current.IsLoaded)
        {
            var oldCtx = new SceneContext(_gd, _logic, _render, _services, Current.World);
            Current.OnUnload(oldCtx);
        }

        // 2) 清空调度器（避免遗留系统）
        _logic.Clear();
        _render.Clear();

        // 3) 加载新场景 + 注册系统
        Current = scene;
        var ctx = new SceneContext(_gd, _logic, _render, _services, scene.World);
        scene.OnLoad(ctx);
        scene.RegisterSystems(ctx);
        scene.MarkLoaded(true);
    }

    public void Reload()
    {
        if (Current is null) return;
        var s = Current;
        Load(s);
    }

    public void OnViewportResize(int width, int height)
    {
        if (Current is null || !Current.IsLoaded) return;
        var ctx = new SceneContext(_gd, _logic, _render, _services, Current.World);
        Current.OnViewportResize(ctx, width, height);
    }
}