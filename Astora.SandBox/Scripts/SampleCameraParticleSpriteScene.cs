using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Rendering;
using Astora.Core.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.SandBox.Scripts
{
    /// <summary>
    /// Example scene demonstrating how to create a scene with Camera, Particle, and Sprite using SceneBuilder.
    /// </summary>
    public static class SampleCameraParticleSpriteScene
    {
        /// <summary>
        /// Creates a sample scene containing a Camera2D, CPUParticles2D, and Sprite using SceneBuilder.
        /// </summary>
        /// <returns>The root Node of the created scene</returns>
        public static Node CreateScene()
        {
            // Create the root node using SceneBuilder
            var scene = SceneBuilder.Create<Node>("SampleScene")

                // Add a Camera2D as a child of the root
                .Add<Camera2D>("MainCamera", camera =>
                {
                    camera.Zoom = 1.0f;
                    // Origin will be automatically set based on viewport when the camera is initialized
                })

                // Add CPUParticles2D for particle effects
                .Add<CPUParticles2D>("ParticleEffects", particles =>
                {
                    particles.Emitting = true;
                    particles.Amount = 50;
                    particles.Lifetime = 2.0f;
                    particles.OneShot = false;
                    
                    // No gravity - particles float in space
                    particles.Gravity = Vector2.Zero;
                    
                    // Direction pointing up-right
                    particles.Direction = new Vector2(1, -1);
                    particles.Spread = 45f;
                    
                    // Random velocity between min and max
                    particles.InitialVelocityMin = 100f;
                    particles.InitialVelocityMax = 200f;
                    
                    // Rotation speed for particles
                    particles.AngularVelocity = 5f;
                    
                    // Color gradient from orange to red/transparent
                    particles.ColorStart = new Color(255, 200, 100, 255);
                    particles.ColorEnd = new Color(255, 50, 50, 0);
                    
                    // Scale animation
                    particles.ScaleStart = 0.5f;
                    particles.ScaleEnd = 0.1f;
                    
                    // Optional: Set emission shape
                    particles.EmissionShape = CPUParticles2D.ParticleEmissionShape.Point;
                    
                    // Optional: Set a texture for particles (uses default if null)
                    // particles.Texture = someTexture;
                })

                // Add a Sprite
                .Add<Sprite>("SampleSprite", sprite =>
                {
                    // Load a texture from content
                    var textureResource = ResourceLoader.Load<Texture2DResource>("Test.png");
                    if (textureResource != null)
                    {
                        sprite.Texture = textureResource.Texture;
                        sprite.TexturePath = textureResource.ResourcePath;
                    }
                    
                    // Origin at center of texture
                    sprite.Origin = new Vector2(70, 70); // Adjust based on texture size
                    
                    // White color (no tinting)
                    sprite.Modulate = Color.White;
                    
                    // No offset
                    sprite.Offset = Vector2.Zero;
                })
                .Build();

            return scene;
        }

        /// <summary>
        /// Creates the same scene and saves it to a .scene file.
        /// </summary>
        /// <param name="outputPath">Path where the scene file will be saved</param>
        public static void CreateAndSaveScene(string outputPath)
        {
            var scene = CreateScene();
            Engine.Serializer.Save(scene, outputPath);
        }
    }
}
