using Microsoft.Xna.Framework;
namespace Astora.Core.Nodes
{
    public class Camera2D : Node2D
    {
        public float Zoom { get; set; } = 1.0f;
        
        public Vector2 Origin { get; set; }

        public Camera2D(string name = "Camera2D") : base(name)
        {
            // There create the origin at the center of the viewport
            if (Engine.GraphicsDevice != null)
            {
                var vp = Engine.GraphicsDevice.Viewport;
                Origin = new Vector2(vp.Width / 2f, vp.Height / 2f);
            }
        }
        public Matrix GetViewMatrix()
        {
            var transform = Matrix.CreateTranslation(new Vector3(-Position, 0)) *
                            Matrix.CreateRotationZ(-Rotation) *
                            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                            Matrix.CreateTranslation(new Vector3(Origin, 0));

            return transform;
        }
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix()));
        }
        
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, GetViewMatrix());
        }
    }
}