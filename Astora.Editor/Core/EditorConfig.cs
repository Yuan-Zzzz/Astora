namespace Astora.Editor.Core;

/// <summary>
/// 编辑器配置
/// </summary>
public class EditorConfig
{
    /// <summary>
    /// 默认窗口宽度
    /// </summary>
    public int DefaultWindowWidth { get; set; } = 1920;
    
    /// <summary>
    /// 默认窗口高度
    /// </summary>
    public int DefaultWindowHeight { get; set; } = 1080;
    
    /// <summary>
    /// 是否允许窗口缩放
    /// </summary>
    public bool AllowWindowResizing { get; set; } = true;
    
    /// <summary>
    /// 背景颜色
    /// </summary>
    public Microsoft.Xna.Framework.Color BackgroundColor { get; set; } = new Microsoft.Xna.Framework.Color(46, 46, 46);
}
