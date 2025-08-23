using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Engine.Components;

public struct Camera2DComponent
{
    public bool Primary;       
    public float Zoom;             
    public float Rotation;     
    public Vector2 Position; 
    public Rectangle Viewport;
    public Matrix View;
    public Matrix Projection;

    public Camera2DComponent(Rectangle vp)
    {
        Primary = true;
        Zoom = 1.0f;
        Rotation = 0f;
        Position = Vector2.Zero;
        Viewport = vp;
        View = Matrix.Identity;
        Projection = Matrix.Identity;
    }
}