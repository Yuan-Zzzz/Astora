using Microsoft.Xna.Framework;
using Astora.Core;
using Astora.Core.Time;
using Astora.ECS;
using Astora.Engine.Components;
using Astora.Engine.Core;

namespace Astora.Engine.Systems;

/// <summary>更新 2D 相机的 View/Projection。</summary>
public struct Camera2DSystem : ILogicSystem
{
    private readonly World _world;
    public int Order => 400; // 渲染前

    public Camera2DSystem(World world) => _world = world;

    public void TickLogic(ITime t)
    {
        Console.WriteLine($"Camera2DSystem.TickLogic: {t.Delta} seconds");
        foreach (var e in _world.Query<Camera2DComponent>())
        {
            ref var cam = ref _world.GetComponent<Camera2DComponent>(e);

            var vp = cam.Viewport;
            var center = new Vector2(vp.Width * 0.5f, vp.Height * 0.5f);

            // 将相机位置居中到屏幕中点：T(-pos) * R(-rot) * S(zoom) * T(center)
            var trans = Matrix.CreateTranslation(-cam.Position.X, -cam.Position.Y, 0);
            var rot   = Matrix.CreateRotationZ(-cam.Rotation);
            var scale = Matrix.CreateScale(cam.Zoom, cam.Zoom, 1f);
            var move  = Matrix.CreateTranslation(center.X, center.Y, 0);

            cam.View = trans * rot * scale * move;

            // 供需要的地方使用（SpriteBatch 的 transformMatrix 通常只用 View）
            cam.Projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
        }
    }
}