using Astora.Core.Utils;
using Astora.Editor.Project;
using Astora.Editor.Core;
using Astora.Core;

namespace Astora.Editor.Services;

/// <summary>
/// 项目服务 - 封装项目相关功能（支持异步加载）
/// </summary>
public class ProjectService
{
    private readonly ProjectManager _projectManager;
    private readonly SceneManager _sceneManager;
    private readonly NodeTypeRegistry _nodeTypeRegistry;

    // 异步加载状态
    private Task? _loadTask;
    private volatile bool _loadCompleted;
    private Action? _pendingMainThreadWork;
    private readonly object _lock = new();

    public ProjectService()
    {
        _projectManager = new ProjectManager();
        _sceneManager = new SceneManager(_projectManager);
        _nodeTypeRegistry = new NodeTypeRegistry();
        _nodeTypeRegistry.DiscoverNodeTypes();
    }

    public ProjectManager ProjectManager => _projectManager;
    public SceneManager SceneManager => _sceneManager;
    public NodeTypeRegistry NodeTypeRegistry => _nodeTypeRegistry;

    /// <summary>
    /// 异步加载项目（不阻塞主线程）
    /// </summary>
    public void LoadProjectAsync(string csprojPath, EditorState state)
    {
        if (state.IsLoadingProject)
            return;

        state.LoadState = ProjectLoadState.Loading;
        state.LoadMessage = "Loading project file...";
        state.LoadProgress = 0f;
        state.LoadError = null;

        _loadCompleted = false;

        _loadTask = Task.Run(() =>
        {
            try
            {
                // 阶段 1：加载项目文件（快速）
                state.LoadMessage = "Loading project file...";
                state.LoadProgress = 0.1f;

                var projectInfo = _projectManager.LoadProject(csprojPath);

                // 阶段 2：设置 Content 路径
                state.LoadMessage = "Configuring content directory...";
                state.LoadProgress = 0.2f;

                var contentRoot = projectInfo.GameConfig?.ContentRootDirectory ?? "Content";
                var projectContentDir = Path.Combine(projectInfo.ProjectRoot, contentRoot);

                // Content 路径设置需要在主线程执行（MonoGame 线程安全问题）
                // 这里先记录下来
                string? contentDir = null;
                if (Directory.Exists(projectContentDir))
                    contentDir = projectContentDir;

                // 阶段 3：初始化和扫描场景
                state.LoadMessage = "Scanning scenes...";
                state.LoadProgress = 0.3f;
                state.LoadState = ProjectLoadState.Loading;

                _sceneManager.Initialize();
                _sceneManager.ScanScenes();

                // 阶段 4：编译项目
                state.LoadMessage = "Compiling project...";
                state.LoadProgress = 0.5f;
                state.LoadState = ProjectLoadState.Compiling;

                var compileResult = _projectManager.CompileProject();

                if (compileResult.Success)
                {
                    // 阶段 5：加载程序集
                    state.LoadMessage = "Loading assembly...";
                    state.LoadProgress = 0.7f;
                    state.LoadState = ProjectLoadState.LoadingAssembly;

                    _projectManager.LoadProjectAssembly();
                    var loadedAssembly = _projectManager.GetLoadedAssembly();

                    // 阶段 6：发现节点类型
                    state.LoadMessage = "Discovering node types...";
                    state.LoadProgress = 0.85f;
                    state.LoadState = ProjectLoadState.DiscoveringNodes;

                    _nodeTypeRegistry.SetPriorityAssembly(loadedAssembly);
                    YamlSceneSerializer.SetPriorityAssembly(loadedAssembly);
                    _nodeTypeRegistry.MarkDirty();
                    _nodeTypeRegistry.DiscoverNodeTypes();
                }
                else
                {
                    System.Console.WriteLine($"编译警告: {compileResult.ErrorMessage}");
                }

                // 把需要在主线程执行的工作排队
                lock (_lock)
                {
                    _pendingMainThreadWork = () =>
                    {
                        if (contentDir != null && Engine.Content != null)
                        {
                            Engine.Content.RootDirectory = contentDir;
                            System.Console.WriteLine($"[ProjectService] Content.RootDirectory => {contentDir}");
                        }
                    };
                }

                state.LoadMessage = "Ready";
                state.LoadProgress = 1.0f;
                state.LoadState = ProjectLoadState.Ready;
                _loadCompleted = true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"加载项目失败: {ex.Message}");
                state.LoadState = ProjectLoadState.Error;
                state.LoadError = ex.Message;
                state.LoadMessage = $"Error: {ex.Message}";
            }
        });
    }

    /// <summary>
    /// 在主线程中调用，完成需要在主线程执行的工作
    /// </summary>
    public bool TryFinishLoadOnMainThread(EditorState state)
    {
        if (!_loadCompleted)
            return false;

        lock (_lock)
        {
            _pendingMainThreadWork?.Invoke();
            _pendingMainThreadWork = null;
        }

        _loadCompleted = false;
        _loadTask = null;

        state.IsProjectLoaded = true;
        return true;
    }

    /// <summary>
    /// 同步加载项目（回退方案）
    /// </summary>
    public bool LoadProject(string csprojPath)
    {
        try
        {
            var projectInfo = _projectManager.LoadProject(csprojPath);

            var contentRoot = projectInfo.GameConfig?.ContentRootDirectory ?? "Content";
            var projectContentDir = Path.Combine(projectInfo.ProjectRoot, contentRoot);
            if (Directory.Exists(projectContentDir) && Engine.Content != null)
            {
                Engine.Content.RootDirectory = projectContentDir;
                System.Console.WriteLine($"[ProjectService] Content.RootDirectory => {Engine.Content.RootDirectory}");
            }

            _sceneManager.Initialize();
            _sceneManager.ScanScenes();

            var compileResult = _projectManager.CompileProject();
            if (compileResult.Success)
            {
                _projectManager.LoadProjectAssembly();
                var loadedAssembly = _projectManager.GetLoadedAssembly();
                _nodeTypeRegistry.SetPriorityAssembly(loadedAssembly);
                YamlSceneSerializer.SetPriorityAssembly(loadedAssembly);
                _nodeTypeRegistry.MarkDirty();
                _nodeTypeRegistry.DiscoverNodeTypes();
            }
            else
            {
                System.Console.WriteLine($"编译警告: {compileResult.ErrorMessage}");
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"加载项目失败: {ex.Message}");
            return false;
        }
    }

    public void CloseProject()
    {
        _projectManager.ClearProject();
        _nodeTypeRegistry.SetPriorityAssembly(null);
        YamlSceneSerializer.SetPriorityAssembly(null);
        _nodeTypeRegistry.MarkDirty();
        _nodeTypeRegistry.DiscoverNodeTypes();

        if (Engine.Content != null)
            Engine.Content.RootDirectory = "Content";
    }

    public bool RebuildProject()
    {
        if (!_projectManager.HasProject)
        {
            System.Console.WriteLine("没有加载的项目");
            return false;
        }

        System.Console.WriteLine("开始重新加载程序集...");

        _nodeTypeRegistry.SetPriorityAssembly(null);
        YamlSceneSerializer.SetPriorityAssembly(null);

        var result = _projectManager.ReloadAssembly();

        if (result)
        {
            var loadedAssembly = _projectManager.GetLoadedAssembly();
            _nodeTypeRegistry.SetPriorityAssembly(loadedAssembly);
            _nodeTypeRegistry.MarkDirty();
            _nodeTypeRegistry.DiscoverNodeTypes();
            YamlSceneSerializer.SetPriorityAssembly(loadedAssembly);
            System.Console.WriteLine("程序集重新加载成功");
        }
        else
        {
            System.Console.WriteLine("程序集重新加载失败");
        }

        return result;
    }
}
