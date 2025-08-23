using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Core;

namespace Astora.Engine.Scene;

public readonly struct SceneContext
{
    public GraphicsDevice GraphicsDevice { get; }
    public LogicScheduler Logic { get; }
    public RenderScheduler Render { get; }
    public IServiceRegistry Services { get; }

    public SceneContext(GraphicsDevice gd, LogicScheduler logic, RenderScheduler render, IServiceRegistry services)
    {
        GraphicsDevice = gd;
        Logic = logic;
        Render = render;
        Services = services;
    }

    public T Get<T>() where T : class => Services.Get<T>();
}