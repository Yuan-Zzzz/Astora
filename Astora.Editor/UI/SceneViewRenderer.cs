using Astora.Core;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.UI;

/// <summary>
/// 场景视图渲染器，负责将场景渲染到 RenderTarget
/// </summary>
public class SceneViewRenderer
{
    private readonly SceneTree _sceneTree;
    private readonly RenderBatcher _renderBatcher;
    private RenderTarget2D? _renderTarget;
    private IntPtr _renderTargetTextureId;
    private readonly ImGuiRenderer _imGuiRenderer;
    
    /// <summary>
    /// 当前 RenderTarget 的宽度
    /// </summary>
    public int Width => _renderTarget?.Width ?? 0;
    
    /// <summary>
    /// 当前 RenderTarget 的高度
    /// </summary>
    public int Height => _renderTarget?.Height ?? 0;
    
    /// <summary>
    /// RenderTarget 是否已创建
    /// </summary>
    public bool IsReady => _renderTarget != null && !_renderTarget.IsDisposed;
    
    /// <summary>
    /// ImGui 纹理 ID，用于在 ImGui 中显示
    /// </summary>
    public IntPtr TextureId => _renderTargetTextureId;
    
    /// <summary>
    /// 当前 RenderTarget（用于绘制覆盖层）
    /// </summary>
    public RenderTarget2D? RenderTarget => _renderTarget;
    
    public SceneViewRenderer(SceneTree sceneTree, ImGuiRenderer imGuiRenderer)
    {
        _sceneTree = sceneTree;
        _imGuiRenderer = imGuiRenderer;
        _renderBatcher = new RenderBatcher(Engine.GDM.GraphicsDevice);
    }
    
    /// <summary>
    /// 更新 RenderTarget 大小
    /// </summary>
    public void UpdateRenderTarget(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;
        
        // 如果大小没有变化，不需要重新创建
        if (_renderTarget != null && 
            _renderTarget.Width == width && 
            _renderTarget.Height == height)
        {
            return;
        }
        
        // 如果纹理ID存在，先解绑
        if (_renderTargetTextureId != IntPtr.Zero)
        {
            _imGuiRenderer.UnbindTexture(_renderTargetTextureId);
            _renderTargetTextureId = IntPtr.Zero;
        }
        
        // 释放旧的 RenderTarget
        _renderTarget?.Dispose();
        
        // 创建新的 RenderTarget
        _renderTarget = new RenderTarget2D(
            Engine.GDM.GraphicsDevice,
            width,
            height
        );
        
        // 绑定 RenderTarget 到 ImGui
        _renderTargetTextureId = _imGuiRenderer.BindTexture(_renderTarget);
    }
    
    /// <summary>
    /// 渲染场景到 RenderTarget
    /// 注意：此方法会设置 RenderTarget，但不会恢复，由调用者负责恢复
    /// </summary>
    public void Draw(SceneViewCamera camera, SpriteBatch spriteBatch)
    {
        if (camera == null || spriteBatch == null)
            return;
        
        if (!IsReady)
            return;
        
        if (Engine.GDM == null || Engine.GDM.GraphicsDevice == null)
            return;
        
        // 设置 RenderTarget
        Engine.GDM.GraphicsDevice.SetRenderTarget(_renderTarget);
        Engine.GDM.GraphicsDevice.Clear(Color.Black);
        
        // 获取视图矩阵
        Matrix viewMatrix = camera.GetViewMatrix();
        
        // 使用 RenderBatcher 渲染场景
        _renderBatcher.Begin(viewMatrix, SamplerState.PointClamp);
        _sceneTree.Draw(_renderBatcher);
        _renderBatcher.End();
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        if (_renderTargetTextureId != IntPtr.Zero)
        {
            _imGuiRenderer.UnbindTexture(_renderTargetTextureId);
            _renderTargetTextureId = IntPtr.Zero;
        }
        
        _renderTarget?.Dispose();
        _renderTarget = null;
    }
}
