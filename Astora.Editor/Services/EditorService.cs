using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Astora.Editor.Core;

namespace Astora.Editor.Services;

/// <summary>
/// 编辑器核心服务 - 管理场景、节点选择等核心功能
/// </summary>
public class EditorService
{
    private readonly SceneTree _sceneTree;
    private readonly EditorState _state;
    private readonly ProjectService _projectService;
    
    public EditorService(ProjectService projectService)
    {
        _projectService = projectService;
        _sceneTree = new SceneTree();
        Engine.CurrentScene = _sceneTree;
        _state = new EditorState();
    }
    
    /// <summary>
    /// 场景树
    /// </summary>
    public SceneTree SceneTree => _sceneTree;
    
    /// <summary>
    /// 编辑器状态
    /// </summary>
    public EditorState State => _state;
    
    /// <summary>
    /// 设置选中的节点
    /// </summary>
    public void SetSelectedNode(Node? node)
    {
        _state.SelectedNode = node;
    }
    
    /// <summary>
    /// 获取选中的节点
    /// </summary>
    public Node? GetSelectedNode()
    {
        return _state.SelectedNode;
    }
    
    /// <summary>
    /// 设置播放状态
    /// </summary>
    public void SetPlaying(bool playing)
    {
        if (playing && !_state.IsPlaying)
        {
            // 开始播放：保存当前场景状态
            SaveSceneSnapshot();
        }
        else if (!playing && _state.IsPlaying)
        {
            // 停止播放：恢复场景到初始状态
            RestoreSceneSnapshot();
        }
        
        _state.IsPlaying = playing;
    }
    
    /// <summary>
    /// 保存场景快照（播放前调用）
    /// </summary>
    private void SaveSceneSnapshot()
    {
        if (_sceneTree.Root == null)
        {
            // 没有场景，无需保存
            return;
        }
        
        try
        {
            // 创建临时文件路径
            var tempDir = Path.GetTempPath();
            var tempFileName = $"astora_scene_snapshot_{Guid.NewGuid()}.scene";
            _state.SavedSceneSnapshotPath = Path.Combine(tempDir, tempFileName);
            
            // 保存场景到临时文件
            Engine.Serializer.Save(_sceneTree.Root, _state.SavedSceneSnapshotPath);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"保存场景快照失败: {ex.Message}");
            _state.SavedSceneSnapshotPath = null;
        }
    }
    
    /// <summary>
    /// 恢复场景快照（停止时调用）
    /// </summary>
    private void RestoreSceneSnapshot()
    {
        if (string.IsNullOrEmpty(_state.SavedSceneSnapshotPath) || !File.Exists(_state.SavedSceneSnapshotPath))
        {
            // 没有快照或文件不存在，无需恢复
            return;
        }
        
        try
        {
            // 从临时文件加载场景
            var restoredScene = Engine.Serializer.Load(_state.SavedSceneSnapshotPath);
            
            // 恢复场景
            _sceneTree.ChangeScene(restoredScene);
            
            // 清除选中的节点（因为节点对象已改变）
            _state.SelectedNode = null;
            
            // 清理临时文件
            try
            {
                File.Delete(_state.SavedSceneSnapshotPath);
            }
            catch
            {
                // 忽略删除失败的错误
            }
            
            _state.SavedSceneSnapshotPath = null;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"恢复场景快照失败: {ex.Message}");
            // 清理临时文件
            try
            {
                if (File.Exists(_state.SavedSceneSnapshotPath))
                {
                    File.Delete(_state.SavedSceneSnapshotPath);
                }
            }
            catch
            {
                // 忽略删除失败的错误
            }
            _state.SavedSceneSnapshotPath = null;
        }
    }
    
    /// <summary>
    /// 加载场景
    /// </summary>
    public void LoadScene(string path)
    {
        var scene = _projectService.SceneManager.LoadScene(path);
        if (scene != null)
        {
            _sceneTree.ChangeScene(scene);
            _state.CurrentScenePath = path;
        }
    }
    
    /// <summary>
    /// 保存场景
    /// </summary>
    public void SaveScene(string? path = null)
    {
        if (_sceneTree.Root == null)
        {
            return;
        }
        
        var savePath = path ?? _state.CurrentScenePath;
        if (string.IsNullOrEmpty(savePath))
        {
            // 如果没有路径，使用当前场景名称
            savePath = _projectService.SceneManager.GetScenePath(_sceneTree.Root.Name);
        }
        
        _projectService.SceneManager.SaveScene(savePath, _sceneTree.Root);
        _state.CurrentScenePath = savePath;
    }
    
    /// <summary>
    /// 创建新场景
    /// </summary>
    public void CreateNewScene(string sceneName)
    {
        var scenePath = _projectService.SceneManager.CreateNewScene(sceneName);
        LoadScene(scenePath);
    }
    
    /// <summary>
    /// 关闭项目时清理
    /// </summary>
    public void OnProjectClosed()
    {
        // 如果正在播放，先停止
        if (_state.IsPlaying)
        {
            SetPlaying(false);
        }
        
        // 清理临时快照文件
        if (!string.IsNullOrEmpty(_state.SavedSceneSnapshotPath) && File.Exists(_state.SavedSceneSnapshotPath))
        {
            try
            {
                File.Delete(_state.SavedSceneSnapshotPath);
            }
            catch
            {
                // 忽略删除失败的错误
            }
            _state.SavedSceneSnapshotPath = null;
        }
        
        _sceneTree.ChangeScene(null);
        _state.CurrentScenePath = null;
        _state.SelectedNode = null;
        _state.IsPlaying = false;
        _state.IsProjectLoaded = false;
    }
}
