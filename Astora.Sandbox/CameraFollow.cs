using Astora.Core;
using Astora.Core.Time;
using Astora.Engine.Components;
using XnaVec2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Sandbox.Scripts;

public sealed class CameraFollow : Behaviour
{
    private readonly Astora.Engine.Entity _target;
    public float LerpRate = 8f;
    public CameraFollow(Astora.Engine.Entity target) => _target = target;

    public override void OnUpdate(ITime t)
    {
        ref var cam = ref GetComponent<Camera2DComponent>();
        ref var tr  = ref GetComponentOf<Transform2D>(_target);

        var goal = new XnaVec2(tr.WorldPosition.X, tr.WorldPosition.Y);
        float alpha = 1f - System.MathF.Exp(-LerpRate * t.Delta);
        cam.Position = cam.Position + (goal - cam.Position) * alpha;
    }
}