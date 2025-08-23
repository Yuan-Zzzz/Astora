using System;
using System.Numerics;
using Astora.Core;
using Astora.Core.Time;
using Astora.ECS;
using Astora.Engine.Components;
using Astora.Engine.Core;

namespace Astora.Engine.Systems;
public struct TransformHierarchy2DSystem : ILogicSystem
{
    private readonly Astora.ECS.World _world;
    public int Order => 250;

    public TransformHierarchy2DSystem(Astora.ECS.World world) => _world = world;

    public void TickLogic(ITime t)
    {
        // 1) 根：Parent == -1
        foreach (var e in _world.Query<Transform2D>())
        {
            ref var tr = ref _world.GetComponent<Transform2D>(e);
            if (tr.Parent != -1) continue;

            tr.WorldPosition = tr.LocalPosition;
            tr.WorldRotation = tr.LocalRotation;
            tr.WorldScale    = tr.LocalScale;
        }

        // 2) 子：多迭代几次覆盖常见深度（可按需要把 4 调大/小）
        for (int iter = 0; iter < 4; iter++)
        {
            foreach (var e in _world.Query<Transform2D>())
            {
                ref var tr = ref _world.GetComponent<Transform2D>(e);
                if (tr.Parent == -1) continue;

                var p = tr.Parent;
                if (!_world.Check<Transform2D>().Contains(p)) continue;

                ref var pt = ref _world.GetComponent<Transform2D>(p);

                // 2D 合成（world = parent ∘ local）
                var sin = MathF.Sin(pt.WorldRotation);
                var cos = MathF.Cos(pt.WorldRotation);

                var scaled = new Vector2(tr.LocalPosition.X * pt.WorldScale.X,
                                         tr.LocalPosition.Y * pt.WorldScale.Y);
                var rotated = new Vector2(
                    scaled.X * cos - scaled.Y * sin,
                    scaled.X * sin + scaled.Y * cos
                );

                tr.WorldPosition = pt.WorldPosition + rotated;
                tr.WorldRotation = pt.WorldRotation + tr.LocalRotation;
                tr.WorldScale    = new Vector2(pt.WorldScale.X * tr.LocalScale.X,
                                               pt.WorldScale.Y * tr.LocalScale.Y);
            }
        }
    }
}
