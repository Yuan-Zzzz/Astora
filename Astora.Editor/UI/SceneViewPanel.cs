using Astora.Core;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Astora.Editor.UI
{
    public class SceneViewPanel
    {
        private SceneTree _sceneTree;
        private RenderTarget2D _sceneRenderTarget;
        private IntPtr _renderTargetTextureId;
        private ImGuiRenderer _imGuiRenderer;
        private Vector2 _cameraPosition;
        private float _cameraZoom = 1.0f;
        
        // Constructor modified to receive ImGuiRenderer
        public SceneViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer)
        {
            _sceneTree = sceneTree;
            _imGuiRenderer = imGuiRenderer;
        }
        
        public void Update(GameTime gameTime)
        {
            // Handle scene view input (pan, zoom)
            // Implement Gizmo interaction logic
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_sceneRenderTarget == null) return;
            
            // Set RenderTarget
            Engine.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            Engine.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Render scene to RenderTarget
            Matrix viewMatrix = Matrix.CreateTranslation(new Vector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: viewMatrix
            );
            
            _sceneTree.Draw(spriteBatch);
            
            spriteBatch.End();
            
            // Restore default RenderTarget
            Engine.GraphicsDevice.SetRenderTarget(null);
        }
        
        public void RenderUI()
        {
            ImGui.Begin("Scene View");
            
            // Toolbar
            if (ImGui.Button("Select"))
            {
                // Switch to select tool
            }
            ImGui.SameLine();
            if (ImGui.Button("Move"))
            {
                // Switch to move tool
            }
            ImGui.SameLine();
            if (ImGui.Button("Rotate"))
            {
                // Switch to rotate tool
            }
            
            // Render scene view
            var viewportSize = ImGui.GetContentRegionAvail();
            if (viewportSize.X > 0 && viewportSize.Y > 0)
            {
                // Create or resize RenderTarget
                if (_sceneRenderTarget == null || 
                    _sceneRenderTarget.Width != (int)viewportSize.X || 
                    _sceneRenderTarget.Height != (int)viewportSize.Y)
                {
                    // If texture ID exists, unbind first
                    if (_renderTargetTextureId != IntPtr.Zero)
                    {
                        _imGuiRenderer.UnbindTexture(_renderTargetTextureId);
                    }
                    
                    _sceneRenderTarget?.Dispose();
                    _sceneRenderTarget = new RenderTarget2D(
                        Engine.GraphicsDevice,
                        (int)viewportSize.X,
                        (int)viewportSize.Y
                    );
                    
                    // Bind RenderTarget to ImGui
                    _renderTargetTextureId = _imGuiRenderer.BindTexture(_sceneRenderTarget);
                }
                
                // Display render result - Fix: use bound texture ID
                ImGui.Image(
                    _renderTargetTextureId,  // Use bound texture ID instead of direct conversion
                    viewportSize,
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One
                );
            }
            
            ImGui.End();
        }
    }
}