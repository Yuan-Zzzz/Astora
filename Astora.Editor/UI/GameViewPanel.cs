using Astora.Core;
using Astora.Core.Project;
using Astora.Core.Scene;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Editor.Core;
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
        private readonly SceneTree _sceneTree;
        private readonly ProjectManager? _projectManager;
        private readonly IEditorContext? _ctx;
        private RenderTarget2D _gameRenderTarget;
        private IntPtr _renderTargetTextureId;
        private readonly ImGuiRenderer _imGuiRenderer;
        private readonly RenderBatcher _renderBatcher;

        public GameViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer, ProjectManager? projectManager = null, IEditorContext? ctx = null)
        {
            _sceneTree = sceneTree;
            _imGuiRenderer = imGuiRenderer;
            _projectManager = projectManager;
            _ctx = ctx;
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
            
            // 设置设计分辨率，使 GetScaleMatrix 等与独立运行一致（此处 RT 即设计分辨率时 scale=1）
            if (config != null)
            {
                Engine.SetDesignResolution(config);
            }
            
            Matrix viewMatrix = Matrix.Identity;
            if (_sceneTree.ActiveCamera != null)
            {
                var cam = _sceneTree.ActiveCamera;
                var originalOrigin = cam.Origin;
                cam.Origin = new Microsoft.Xna.Framework.Vector2(
                    _gameRenderTarget.Width / 2f,
                    _gameRenderTarget.Height / 2f
                );
                viewMatrix = cam.GetViewMatrix();
                cam.Origin = originalOrigin;
            }

            var context = new RenderContext
            {
                GraphicsDevice = Engine.GDM.GraphicsDevice,
                RenderBatcher = _renderBatcher,
                CurrentScene = _sceneTree,
                ActiveCamera = _sceneTree.ActiveCamera,
                ViewMatrix = viewMatrix,
                UIMatrix = null,
                WhiteTextureProvider = null
            };
            _sceneTree.Draw(context);
            
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

                if (_ctx != null)
                {
                    var state = _ctx.EditorService.State;
                    state.LastGameViewHovered = ImGui.IsItemHovered();
                    if (state.LastGameViewHovered)
                    {
                        var mouse = ImGui.GetMousePos();
                        var itemMin = ImGui.GetItemRectMin();
                        var itemMax = ImGui.GetItemRectMax();
                        var itemSize = new Vector2(itemMax.X - itemMin.X, itemMax.Y - itemMin.Y);
                        if (itemSize.X > 0 && itemSize.Y > 0)
                        {
                            state.LastGameViewMouseInDesign = new Microsoft.Xna.Framework.Vector2(
                                (float)((mouse.X - itemMin.X) / itemSize.X * designWidth),
                                (float)((mouse.Y - itemMin.Y) / itemSize.Y * designHeight)
                            );
                        }
                        else
                            state.LastGameViewMouseInDesign = null;
                    }
                    else
                        state.LastGameViewMouseInDesign = null;
                }
            }
            
            ImGui.End();
        }
    }
}

