using Astora.Core.Nodes;

namespace Astora.Editor.Core;

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
}
