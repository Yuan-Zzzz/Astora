using Astora.Core;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.UI;

/// <summary>
/// 场景视图渲染器，RT 为面板视口尺寸，场景用编辑器相机，UI 用 design→viewport 缩放。
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
    /// 渲染场景与 UI 到 RenderTarget。RT 为视口尺寸，世界用编辑器相机，UI 用 design→viewport 缩放。
    /// </summary>
    public void Draw(SceneViewCamera camera, SpriteBatch spriteBatch, int designWidth, int designHeight)
    {
        if (camera == null || spriteBatch == null || !IsReady || Engine.GDM?.GraphicsDevice == null || _renderTarget == null)
            return;

        int w = _renderTarget.Width, h = _renderTarget.Height;
        if (designWidth <= 0 || designHeight <= 0) { designWidth = w; designHeight = h; }

        Engine.GDM.GraphicsDevice.SetRenderTarget(_renderTarget);
        Engine.GDM.GraphicsDevice.Clear(Color.Black);
        var vp = Engine.GDM.GraphicsDevice.Viewport;
        Engine.GDM.GraphicsDevice.Viewport = new Viewport(0, 0, w, h);

        var uiScale = Matrix.CreateScale((float)w / designWidth, (float)h / designHeight, 1f);
        var context = new RenderContext
        {
            GraphicsDevice = Engine.GDM.GraphicsDevice,
            RenderBatcher = _renderBatcher,
            CurrentScene = _sceneTree,
            ActiveCamera = null,
            ViewMatrix = camera.GetViewMatrix(),
            UIMatrix = uiScale,
            WhiteTextureProvider = null
        };
        _sceneTree.Draw(context);

        Engine.GDM.GraphicsDevice.Viewport = vp;
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
