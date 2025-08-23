using System.Numerics;

namespace Astora.Engine.Components;
public struct Transform2D
{
    public Vector2 LocalPosition;
    public float   LocalRotation;
    public Vector2 LocalScale;

    public int Parent;
    
    public Vector2 WorldPosition;
    public float   WorldRotation;
    public Vector2 WorldScale;

    public static Transform2D Identity => new()
    {
        LocalPosition = Vector2.Zero,
        LocalRotation = 0f,
        LocalScale    = Vector2.One,
        Parent        = -1,
        WorldPosition = Vector2.Zero,
        WorldRotation = 0f,
        WorldScale    = Vector2.One
    };
}