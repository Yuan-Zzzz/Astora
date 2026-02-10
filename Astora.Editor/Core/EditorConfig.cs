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
    /// 背景颜色（Godot 黑灰主题背景）
    /// </summary>
    public Microsoft.Xna.Framework.Color BackgroundColor { get; set; } = new Microsoft.Xna.Framework.Color(35, 35, 35);

    /// <summary>
    /// UI 缩放因子（0 = 自动检测 DPI，> 0 = 手动覆盖）
    /// </summary>
    public float UiScale { get; set; } = 0f;

    /// <summary>
    /// 基础字号（会乘以 UiScale）
    /// </summary>
    public float BaseFontSize { get; set; } = 18.0f;
}
