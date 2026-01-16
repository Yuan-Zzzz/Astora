using Astora.Core;
using Astora.Core.Project;
using Astora.Core.Scene;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Editor.Project;
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
        private ProjectManager? _projectManager;
        private RenderTarget2D _gameRenderTarget;
        private IntPtr _renderTargetTextureId;
        private ImGuiRenderer _imGuiRenderer;
        private RenderBatcher _renderBatcher;
        
        public GameViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer, ProjectManager? projectManager = null)
        {
            _sceneTree = sceneTree;
            _imGuiRenderer = imGuiRenderer;
            _projectManager = projectManager;
            _renderBatcher = new RenderBatcher(Engine.GDM.GraphicsDevice);
        }
        
        /// <summary>
        /// 渲染游戏视图到RenderTarget
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_gameRenderTarget == null) return;
            
            // 获取项目配置（如果可用）
            var config = _projectManager?.CurrentProject?.GameConfig;
            
            // 保存原始状态
            var originalViewport = Engine.GDM.GraphicsDevice.Viewport;
            var originalDesignResolution = Engine.DesignResolution;
            var originalScalingMode = Engine.ScalingMode;
            
            // 设置RenderTarget
            Engine.GDM.GraphicsDevice.SetRenderTarget(_gameRenderTarget);
            Engine.GDM.GraphicsDevice.Clear(Color.Black);
            
            // 设置视口以匹配RenderTarget大小
            Engine.GDM.GraphicsDevice.Viewport = new Viewport(0, 0, _gameRenderTarget.Width, _gameRenderTarget.Height);
            
            // 如果使用设计分辨率，设置引擎的设计分辨率（用于后续可能的缩放计算）
            if (config != null)
            {
                Engine.SetDesignResolution(config);
            }
            
            // 使用与 Engine.Render() 完全相同的渲染逻辑
            // 计算变换矩阵：先应用缩放，再应用相机视图
            Matrix scaleMatrix = Engine.GetScaleMatrix();
            Matrix viewMatrix = Matrix.Identity;
            if (_sceneTree.ActiveCamera != null)
            {
                viewMatrix = _sceneTree.ActiveCamera.GetViewMatrix();
            }
            
            // 组合变换矩阵：缩放 * 视图（与实际运行时一致）
            Matrix transformMatrix = scaleMatrix * viewMatrix;
            
            // 使用RenderBatcher渲染场景（与实际运行时一致）
            _renderBatcher.Begin(transformMatrix, SamplerState.PointClamp);
            _sceneTree.Draw(_renderBatcher);
            _renderBatcher.End();
            
            // 恢复原始状态
            Engine.GDM.GraphicsDevice.Viewport = originalViewport;
            Engine.SetDesignResolution(originalDesignResolution.X, originalDesignResolution.Y, originalScalingMode);
            
            // 恢复默认RenderTarget
            Engine.GDM.GraphicsDevice.SetRenderTarget(null);
        }
        
        /// <summary>
        /// 渲染UI窗口
        /// </summary>
        public void RenderUI()
        {
            ImGui.Begin("Game");
            
            // 获取项目配置以确定设计分辨率
            var config = _projectManager?.CurrentProject?.GameConfig;
            int designWidth = config?.DesignWidth ?? 1920;
            int designHeight = config?.DesignHeight ?? 1080;
            
            // 渲染游戏视图
            var viewportSize = ImGui.GetContentRegionAvail();
            if (viewportSize.X > 0 && viewportSize.Y > 0)
            {
                // 根据设计分辨率和缩放模式计算RenderTarget大小
                int renderWidth, renderHeight;
                
                if (config != null && config.ScalingMode != ScalingMode.None)
                {
                    // 使用设计分辨率作为RenderTarget大小
                    renderWidth = designWidth;
                    renderHeight = designHeight;
                }
                else
                {
                    // 使用视口大小
                    renderWidth = (int)viewportSize.X;
                    renderHeight = (int)viewportSize.Y;
                }
                
                // 创建或调整RenderTarget大小
                if (_gameRenderTarget == null || 
                    _gameRenderTarget.Width != renderWidth || 
                    _gameRenderTarget.Height != renderHeight)
                {
                    // 如果纹理ID存在，先解绑
                    if (_renderTargetTextureId != IntPtr.Zero)
                    {
                        _imGuiRenderer.UnbindTexture(_renderTargetTextureId);
                    }
                    
                    _gameRenderTarget?.Dispose();
                    _gameRenderTarget = new RenderTarget2D(
                        Engine.GDM.GraphicsDevice,
                        renderWidth,
                        renderHeight
                    );
                    
                    // 绑定RenderTarget到ImGui
                    _renderTargetTextureId = _imGuiRenderer.BindTexture(_gameRenderTarget);
                }
                
                // 计算显示大小（根据缩放模式）
                Vector2 displaySize = viewportSize;
                if (config != null && config.ScalingMode != ScalingMode.None)
                {
                    // 计算缩放以适配视口
                    float scaleX = viewportSize.X / designWidth;
                    float scaleY = viewportSize.Y / designHeight;
                    float scale = Math.Min(scaleX, scaleY);
                    
                    displaySize = new Vector2(designWidth * scale, designHeight * scale);
                }
                
                // 显示渲染结果
                ImGui.Image(
                    _renderTargetTextureId,
                    displaySize,
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One
                );
            }
            
            ImGui.End();
        }
    }
}

