// Astora.Sandbox/Scenes/DemoScene.cs

using Astora.Core.Diagnostics;
using Astora.Engine.Components;
using Astora.Engine.Graphics;
using Astora.Engine.Scene;
using Astora.Engine.Systems;
using Astora.Sandbox.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Sandbox;

public sealed class DemoScene : Scene
{
    private Texture2D? _pixel;
    private int _player;
    private int _camera;

    public DemoScene() : base("Demo") { }

    public override void OnLoad(SceneContext ctx)
    {
        var vp = ctx.GraphicsDevice.Viewport.Bounds;

        // 使用封装的 Entity API
        var camera = ctx.CreateEntity();
        ctx.AddComponent(camera, new Camera2DComponent(vp)
        {
            Primary = true,
            Zoom = 1.0f,
            Position = new Vector2(vp.Width / 2f, vp.Height / 2f)
        });

        _pixel = new Texture2D(ctx.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        var player = ctx.CreateEntity();
        ctx.AddComponent(player, new Transform2D
        {
            LocalPosition = new System.Numerics.Vector2(200, 200),
            LocalScale = new System.Numerics.Vector2(32, 32),
            LocalRotation = 0f,
            Parent = -1
        });
        ctx.AddComponent(player, new SpriteRendererComponent(_pixel, Color.Salmon, RenderLayer.World)
        {
            SortKey = 0.5f
        });
        ctx.AddComponent(player, new TagComponent("Player"));
        ctx.AddComponent(player, new ScriptComponent(() => new PlayerController()));

        ctx.AddComponent(camera, new ScriptComponent(() => new CameraFollow(player)));

        // 保存内部用的原始 id（如果后续系统仍旧直接依赖 int）
        _camera = camera.Id;
        _player = player.Id;
    }

    public override void RegisterSystems(SceneContext ctx)
    {
        ctx.Logic.Add(new TransformHierarchy2DSystem(World));                       
        ctx.Logic.Add(new ScriptSystem(World));                     
        ctx.Logic.Add(new Camera2DSystem(World));                           

        ctx.Render.Add(new ClearScreenSystem(ctx.GraphicsDevice){ ClearColor = Color.CornflowerBlue });
        ctx.Render.Add(new SpriteRenderSystem(World, ctx.GraphicsDevice));
    }

    public override void OnViewportResize(SceneContext ctx, int width, int height)
    {
        foreach (var e in ctx.Query<Camera2DComponent>())
        {
            ref var cam = ref ctx.GetComponent<Camera2DComponent>(e);
            cam.Viewport = new Rectangle(0, 0, width, height);
        }
    }

    public override void OnUnload(SceneContext ctx)
    {
        _pixel?.Dispose(); _pixel = null;
    }
}
