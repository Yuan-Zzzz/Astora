using Astora.Core;
using Astora.Core.Time;
using Astora.Engine.Components;
using XnaVec2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Sandbox.Scripts;

public sealed class CameraFollow : Behaviour
{
    private readonly int _target;
    public float LerpRate = 8f;
    public CameraFollow(int target) => _target = target;

    public override void OnUpdate(ITime t)
    {
        ref var cam = ref World!.GetComponent<Camera2DComponent>(Entity);
        ref var tr  = ref World!.GetComponent<Transform2D>(_target);

        var goal = new XnaVec2(tr.WorldPosition.X, tr.WorldPosition.Y);
        float alpha = 1f - System.MathF.Exp(-LerpRate * t.Delta);
        cam.Position = cam.Position + (goal - cam.Position) * alpha;
    }
}