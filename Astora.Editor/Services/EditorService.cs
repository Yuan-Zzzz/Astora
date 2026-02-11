using Astora.Core;
using Astora.Core.Game;
using Astora.Core.Nodes;
using Astora.Core.Project;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Astora.Editor.Core;
using Astora.Editor.Project;

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
    /// 项目 IGameRuntime 实例（播放时创建，与独立运行共用同一套逻辑）
    /// </summary>
    public IGameRuntime? GameRuntime { get; private set; }
    
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
            SaveSceneSnapshot();
            CreateGameRuntimeIfAvailable();
        }
        else if (!playing && _state.IsPlaying)
        {
            GameRuntime = null;
            RestoreSceneSnapshot();
        }
        
        _state.IsPlaying = playing;
    }

    private void CreateGameRuntimeIfAvailable()
    {
        var projectInfo = _projectService.ProjectManager.CurrentProject;
        var runtimeType = projectInfo?.GameRuntimeType;
        if (runtimeType == null || Engine.Content == null)
            return;

        try
        {
            var runtime = (IGameRuntime?)Activator.CreateInstance(runtimeType);
            if (runtime != null)
            {
                var config = projectInfo.GameConfig ?? GameProjectConfig.CreateDefault();
                runtime.Initialize(Engine.Content, config, _sceneTree, skipInitialSceneLoad: true);
                GameRuntime = runtime;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"创建 IGameRuntime 失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 保存场景快照（播放前调用）—— 暂保留 YAML 序列化用于临时快照
    /// </summary>
    private void SaveSceneSnapshot()
    {
        if (_sceneTree.Root == null)
            return;
        
        try
        {
            var tempDir = Path.GetTempPath();
            var tempFileName = $"astora_scene_snapshot_{Guid.NewGuid()}.scene";
            _state.SavedSceneSnapshotPath = Path.Combine(tempDir, tempFileName);
            
            // 使用 YAML 序列化器保存临时快照
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
            return;
        
        try
        {
            var restoredScene = Engine.Serializer.Load(_state.SavedSceneSnapshotPath);
            _sceneTree.ChangeScene(restoredScene);
            _state.SelectedNode = null;
            
            try { File.Delete(_state.SavedSceneSnapshotPath); } catch { }
            _state.SavedSceneSnapshotPath = null;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"恢复场景快照失败: {ex.Message}");
            try
            {
                if (File.Exists(_state.SavedSceneSnapshotPath))
                    File.Delete(_state.SavedSceneSnapshotPath);
            }
            catch { }
            _state.SavedSceneSnapshotPath = null;
        }
    }
    
    /// <summary>
    /// 加载场景（Code-as-Scene: 通过反射调用 IScene.Build()）
    /// </summary>
    public void LoadScene(SceneInfo sceneInfo)
    {
        var scene = _projectService.SceneManager.LoadScene(sceneInfo);
        if (scene != null)
        {
            _sceneTree.ChangeScene(scene);
            _state.CurrentScene = sceneInfo;
            _state.NotificationManager.ShowSuccess($"场景 '{sceneInfo.ClassName}' 加载成功");
            System.Console.WriteLine($"场景已加载: {sceneInfo.ClassName}");
        }
        else
        {
            _state.NotificationManager.ShowError($"加载场景失败，请查看控制台了解详细信息");
            System.Console.WriteLine($"加载场景失败: {sceneInfo.ClassName}");
        }
    }
    
    /// <summary>
    /// 保存场景（Code-as-Scene: 生成 C# 代码写入 .scene.cs）
    /// </summary>
    public void SaveScene()
    {
        if (_sceneTree.Root == null)
        {
            _state.NotificationManager.ShowError("无法保存：没有场景可保存");
            return;
        }
        
        var sceneInfo = _state.CurrentScene;
        if (sceneInfo == null)
        {
            // 没有关联的 SceneInfo，用根节点名创建一个
            var className = _sceneTree.Root.Name;
            sceneInfo = new SceneInfo
            {
                ClassName = className,
                ScenePath = $"Scenes/{className}",
                SourceFilePath = null // SceneManager will determine the path
            };
            _state.CurrentScene = sceneInfo;
        }
        
        var result = _projectService.SceneManager.SaveScene(sceneInfo, _sceneTree.Root);
        
        if (result)
        {
            _state.NotificationManager.ShowSuccess($"场景 '{sceneInfo.ClassName}' 保存成功");
            System.Console.WriteLine($"场景已保存: {sceneInfo.ClassName}");
        }
        else
        {
            _state.NotificationManager.ShowError($"保存场景失败，请查看控制台了解详细信息");
        }
    }
    
    /// <summary>
    /// 创建新场景
    /// </summary>
    public void CreateNewScene(string sceneName)
    {
        try
        {
            var sceneInfo = _projectService.SceneManager.CreateNewScene(sceneName);
            
            // Load the newly created scene (it has a root Node)
            var rootNode = new Node(sceneInfo.ClassName);
            _sceneTree.ChangeScene(rootNode);
            _state.CurrentScene = sceneInfo;
            
            System.Console.WriteLine($"新场景已创建: {sceneInfo.ClassName}");
        }
        catch (Exception ex)
        {
            _state.NotificationManager.ShowError($"创建场景失败: {ex.Message}");
            System.Console.WriteLine($"创建场景失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 关闭项目时清理
    /// </summary>
    public void OnProjectClosed()
    {
        GameRuntime = null;
        if (_state.IsPlaying)
            SetPlaying(false);
        
        // 清理临时快照文件
        if (!string.IsNullOrEmpty(_state.SavedSceneSnapshotPath) && File.Exists(_state.SavedSceneSnapshotPath))
        {
            try { File.Delete(_state.SavedSceneSnapshotPath); } catch { }
            _state.SavedSceneSnapshotPath = null;
        }
        
        _sceneTree.ChangeScene(null);
        _state.CurrentScene = null;
        _state.SelectedNode = null;
        _state.IsPlaying = false;
        _state.IsProjectLoaded = false;
    }
}
