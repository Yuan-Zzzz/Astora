using Astora.Core;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.Services;

/// <summary>
/// 渲染服务 - 管理渲染相关的资源
/// </summary>
public class RenderService
{
    private RenderBatcher? _renderBatcher;
    private SpriteBatch? _spriteBatch;
    
    /// <summary>
    /// 获取或创建RenderBatcher
    /// </summary>
    public RenderBatcher GetRenderBatcher()
    {
        if (_renderBatcher == null && Engine.GDM?.GraphicsDevice != null)
        {
            _renderBatcher = new RenderBatcher(Engine.GDM.GraphicsDevice);
        }
        return _renderBatcher!;
    }
    
    /// <summary>
    /// 获取或创建SpriteBatch
    /// </summary>
    public SpriteBatch GetSpriteBatch()
    {
        if (_spriteBatch == null && Engine.GDM?.GraphicsDevice != null)
        {
            _spriteBatch = new SpriteBatch(Engine.GDM.GraphicsDevice);
        }
        return _spriteBatch!;
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        _spriteBatch?.Dispose();
        _spriteBatch = null;
        _renderBatcher = null;
    }
}
