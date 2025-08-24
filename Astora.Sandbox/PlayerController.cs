using Microsoft.Xna.Framework.Input;
using Astora.Core;
using Astora.Core.Time;
using Astora.Engine.Components;

namespace Astora.Sandbox.Scripts;

public sealed class PlayerController : Behaviour
{
    public float Speed = 220f;

    public override void OnUpdate(ITime t)
    {
        var ks = Keyboard.GetState();
        int dx = 0, dy = 0;
        if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))  dx -= 1;
        if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right)) dx += 1;
        if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))    dy -= 1;
        if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))  dy += 1;

        if (dx == 0 && dy == 0) return;

        var len = System.MathF.Sqrt(dx * dx + dy * dy);
        var dir = new System.Numerics.Vector2(dx / len, dy / len);

        ref var tr = ref GetComponent<Transform2D>();
        tr.LocalPosition += dir * (Speed * t.Delta);
    }
}