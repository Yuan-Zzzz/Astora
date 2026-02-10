using Astora.Core.Nodes;

namespace Astora.Editor.Core;

/// <summary>
/// 项目加载状态
/// </summary>
public enum ProjectLoadState
{
    Idle,
    Loading,
    Compiling,
    LoadingAssembly,
    DiscoveringNodes,
    Ready,
    Error
}

/// <summary>
/// 编辑器状态管理
/// </summary>
public class EditorState
{
    /// <summary>
    /// 当前选中的节点
    /// </summary>
    public Node? SelectedNode { get; set; }
    
    /// <summary>
    /// 是否正在播放
    /// </summary>
    public bool IsPlaying { get; set; }
    
    /// <summary>
    /// 当前场景路径
    /// </summary>
    public string? CurrentScenePath { get; set; }
    
    /// <summary>
    /// 项目是否已加载
    /// </summary>
    public bool IsProjectLoaded { get; set; }
    
    /// <summary>
    /// 场景快照路径（用于播放前保存场景状态）
    /// </summary>
    public string? SavedSceneSnapshotPath { get; set; }
    
    /// <summary>
    /// 通知管理器
    /// </summary>
    public NotificationManager NotificationManager { get; } = new NotificationManager();

    // === 项目加载进度 ===

    /// <summary>
    /// 项目加载状态
    /// </summary>
    public ProjectLoadState LoadState { get; set; } = ProjectLoadState.Idle;

    /// <summary>
    /// 加载进度消息（UI 显示用）
    /// </summary>
    public string LoadMessage { get; set; } = string.Empty;

    /// <summary>
    /// 加载进度 (0.0 ~ 1.0)
    /// </summary>
    public float LoadProgress { get; set; } = 0f;

    /// <summary>
    /// 加载错误信息
    /// </summary>
    public string? LoadError { get; set; }

    /// <summary>
    /// 是否正在加载项目（非阻塞检测）
    /// </summary>
    public bool IsLoadingProject => LoadState is ProjectLoadState.Loading
        or ProjectLoadState.Compiling
        or ProjectLoadState.LoadingAssembly
        or ProjectLoadState.DiscoveringNodes;
}
