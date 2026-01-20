using Astora.Core.Attributes;
using Microsoft.Xna.Framework;

namespace Astora.Core.Nodes
{
    public class Camera2D : Node2D
    {
        [SerializeField]
        private float _zoom = 1.0f;
        
        [SerializeField]
        private Vector2 _origin;
        
        public float Zoom 
        { 
            get => _zoom; 
            set => _zoom = value; 
        }
        
        public Vector2 Origin 
        { 
            get => _origin; 
            set => _origin = value; 
        }
        
        public Camera2D() : base()
        {
            ResizeViewport();
        }

        public Camera2D(string name = "Camera2D") : base(name)
        {
            ResizeViewport();
        }
        public Matrix GetViewMatrix()
        {
            var transform = 
                            Matrix.CreateTranslation(new Vector3(-Position, 0)) *
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
        
        public Vector2 GetCameraBounds()
        {
            if (Engine.GDM.GraphicsDevice == null)
                return Vector2.Zero;

            var width = Engine.DesignResolution.X / Zoom;
            var height = Engine.DesignResolution.Y / Zoom;
            return new Vector2(width, height);
        }
        
        /// <summary>
        /// Resize the viewport and update the camera origin by GraphicsDevice viewport size 
        /// </summary>
        public void ResizeViewport()
        {
            // There create the origin at the center of the viewport
            if (Engine.GDM.GraphicsDevice != null)
            {
                var ds = Engine.DesignResolution;
                _origin = new Vector2(ds.X / 2f, ds.Y / 2f);
            }
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            ResizeViewport();
        }
    }
}
