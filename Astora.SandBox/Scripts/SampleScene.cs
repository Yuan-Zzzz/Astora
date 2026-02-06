using Astora.Core.Nodes;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scripts
{
    public class SampleScene: IScene
    {
        public static string ScenePath => "Scenes/Test666.scene";
        public static Node Build()
        {
            var scene = SceneBuilder.Create<Node>("SampleScene")
                .Add<Camera2D>("MainCamera", camera =>
                {
                    camera.Zoom = 1.0f;
                })
                .Add<CPUParticles2D>("ParticleEffects", particles =>
                {
                    particles.Emitting = true;
                    particles.Amount = 50;
                    particles.Lifetime = 2.0f;
                    particles.OneShot = false;
                    particles.Gravity = Vector2.Zero;
                    particles.Direction = new Vector2(1, -1);
                    particles.Spread = 45f;
                    particles.InitialVelocityMin = 100f;
                    particles.InitialVelocityMax = 200f;
                    particles.AngularVelocity = 5f;
                    particles.ColorStart = new Color(255, 200, 100, 255);
                    particles.ColorEnd = new Color(255, 50, 50, 0);
                    particles.ScaleStart = 0.5f;
                    particles.ScaleEnd = 0.1f;
                    particles.EmissionShape = CPUParticles2D.ParticleEmissionShape.Point;
                })
                .Add<Sprite>("SampleSprite", sprite =>
                {
                    var textureResource = ResourceLoader.Load<Texture2DResource>("Test.png");
                    if (textureResource != null)
                    {
                        sprite.Texture = textureResource.Texture;
                        sprite.TexturePath = textureResource.ResourcePath;
                    }
                    sprite.Origin = new Vector2(70, 70); // Adjust based on texture size
                    sprite.Modulate = Color.White;
                    sprite.Offset = Vector2.Zero;
                })
                .Build();

            return scene;
        }
        
    }
}
