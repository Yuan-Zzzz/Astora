using Astora.Core;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Astora.Editor.UI
{
    /// <summary>
    /// Game视图面板 - 在播放模式下显示游戏实际渲染结果
    /// </summary>
    public class GameViewPanel
    {
        private SceneTree _sceneTree;
        private RenderTarget2D _gameRenderTarget;
        private IntPtr _renderTargetTextureId;
        private ImGuiRenderer _imGuiRenderer;
        
        public GameViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer)
        {
            _sceneTree = sceneTree;
            _imGuiRenderer = imGuiRenderer;
        }
        
        /// <summary>
        /// 渲染游戏视图到RenderTarget
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_gameRenderTarget == null) return;
            
            // 设置RenderTarget
            Engine.GraphicsDevice.SetRenderTarget(_gameRenderTarget);
            Engine.GraphicsDevice.Clear(Color.Black);
            
            // 使用Engine.Render()渲染场景（使用场景的ActiveCamera）
            // 但需要临时设置SpriteBatch的变换矩阵
            Matrix viewMatrix = Matrix.Identity;
            if (_sceneTree.ActiveCamera != null)
            {
                viewMatrix = _sceneTree.ActiveCamera.GetViewMatrix();
            }
            
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: viewMatrix
            );
            
            _sceneTree.Draw(spriteBatch);
            
            spriteBatch.End();
            
            // 恢复默认RenderTarget
            Engine.GraphicsDevice.SetRenderTarget(null);
        }
        
        /// <summary>
        /// 渲染UI窗口
        /// </summary>
        public void RenderUI()
        {
            ImGui.Begin("Game");
            
            // 渲染游戏视图
            var viewportSize = ImGui.GetContentRegionAvail();
            if (viewportSize.X > 0 && viewportSize.Y > 0)
            {
                // 创建或调整RenderTarget大小
                if (_gameRenderTarget == null || 
                    _gameRenderTarget.Width != (int)viewportSize.X || 
                    _gameRenderTarget.Height != (int)viewportSize.Y)
                {
                    // 如果纹理ID存在，先解绑
                    if (_renderTargetTextureId != IntPtr.Zero)
                    {
                        _imGuiRenderer.UnbindTexture(_renderTargetTextureId);
                    }
                    
                    _gameRenderTarget?.Dispose();
                    _gameRenderTarget = new RenderTarget2D(
                        Engine.GraphicsDevice,
                        (int)viewportSize.X,
                        (int)viewportSize.Y
                    );
                    
                    // 绑定RenderTarget到ImGui
                    _renderTargetTextureId = _imGuiRenderer.BindTexture(_gameRenderTarget);
                }
                
                // 显示渲染结果
                ImGui.Image(
                    _renderTargetTextureId,
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

