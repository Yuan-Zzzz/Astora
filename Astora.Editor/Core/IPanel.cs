using Microsoft.Xna.Framework;

namespace Astora.Editor.Core;

/// <summary>
/// 面板接口 - 统一所有UI面板的接口
/// </summary>
public interface IPanel
{
    /// <summary>
    /// 面板名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 是否可见
    /// </summary>
    bool IsVisible { get; set; }
    
    /// <summary>
    /// 更新面板（如果需要）
    /// </summary>
    void Update(GameTime gameTime);
    
    /// <summary>
    /// 渲染面板UI
    /// </summary>
    void Render();
}
