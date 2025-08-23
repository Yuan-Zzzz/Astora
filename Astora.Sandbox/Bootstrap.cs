using Microsoft.Xna.Framework.Graphics;
using Astora.Core.Diagnostics;
using Astora.Engine.Core;
using Astora.Engine.Scene;

namespace Astora.Sandbox;

public static class Bootstrap
{
    public static SceneManager Configure(IServiceRegistry services, GraphicsDevice gd)
    {
        services.Add<ILogger>(new ConsoleLogger { Level = LogLevel.Debug });

        var logic  = new LogicScheduler();
        var render = new RenderScheduler();
        services.Add(logic);
        services.Add(render);
        var scenes = new SceneManager(services, gd, logic, render);
        services.Add(scenes);

        // 加载 DemoScene
        scenes.Load(new DemoScene());
        return scenes;
    }
}