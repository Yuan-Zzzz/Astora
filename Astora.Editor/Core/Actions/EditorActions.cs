using Astora.Core.Nodes;
using Astora.Editor.Services;

namespace Astora.Editor.Core.Actions;

/// <summary>
/// 默认动作实现：把 UI 行为路由到 ProjectService / EditorService。
/// </summary>
public sealed class EditorActions : IEditorActions
{
    private readonly ProjectService _projectService;
    private readonly EditorService _editorService;

    public EditorActions(ProjectService projectService, EditorService editorService)
    {
        _projectService = projectService;
        _editorService = editorService;
    }

    public bool LoadProject(string csprojPath)
    {
        // 使用异步加载
        _projectService.LoadProjectAsync(csprojPath, _editorService.State);
        return true; // 立即返回，加载在后台进行
    }

    public void CloseProject()
    {
        _projectService.CloseProject();
        _editorService.OnProjectClosed();
    }

    public bool RebuildProject()
    {
        if (!_projectService.ProjectManager.HasProject)
        {
            System.Console.WriteLine("没有加载的项目");
            return false;
        }

        var savedScenePath = _editorService.State.CurrentScenePath;
        var ok = _projectService.RebuildProject();

        if (ok && !string.IsNullOrEmpty(savedScenePath) && File.Exists(savedScenePath))
        {
            System.Console.WriteLine($"重新加载场景: {savedScenePath}");
            _editorService.LoadScene(savedScenePath);
        }

        return ok;
    }

    /// <summary>
    /// 在主线程 Update 中检查异步加载是否完成
    /// </summary>
    public void PollAsyncLoad()
    {
        if (_editorService.State.IsLoadingProject || _editorService.State.LoadState == ProjectLoadState.Ready)
        {
            if (_projectService.TryFinishLoadOnMainThread(_editorService.State))
            {
                _editorService.State.LoadState = ProjectLoadState.Idle;
                _editorService.State.NotificationManager.ShowSuccess("Project loaded successfully");
            }
        }
    }

    public void LoadScene(string path) => _editorService.LoadScene(path);
    public void SaveScene(string? path = null) => _editorService.SaveScene(path);
    public void CreateNewScene(string sceneName) => _editorService.CreateNewScene(sceneName);

    public void SetPlaying(bool playing) => _editorService.SetPlaying(playing);

    public Node? GetSelectedNode() => _editorService.GetSelectedNode();
    public void SetSelectedNode(Node? node) => _editorService.SetSelectedNode(node);
}
