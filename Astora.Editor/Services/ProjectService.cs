using Astora.Core.Utils;
using Astora.Editor.Project;
using Astora.Core;

namespace Astora.Editor.Services;

/// <summary>
/// 项目服务 - 封装项目相关功能
/// </summary>
public class ProjectService
{
    private readonly ProjectManager _projectManager;
    private readonly SceneManager _sceneManager;
    private readonly NodeTypeRegistry _nodeTypeRegistry;
    
    public ProjectService()
    {
        _projectManager = new ProjectManager();
        _sceneManager = new SceneManager(_projectManager);
        _nodeTypeRegistry = new NodeTypeRegistry();
        _nodeTypeRegistry.DiscoverNodeTypes();
    }
    
    /// <summary>
    /// 项目管理器
    /// </summary>
    public ProjectManager ProjectManager => _projectManager;
    
    /// <summary>
    /// 场景管理器
    /// </summary>
    public SceneManager SceneManager => _sceneManager;
    
    /// <summary>
    /// 节点类型注册表
    /// </summary>
    public NodeTypeRegistry NodeTypeRegistry => _nodeTypeRegistry;
    
    /// <summary>
    /// 加载项目
    /// </summary>
    public bool LoadProject(string csprojPath)
    {
        try
        {
            var projectInfo = _projectManager.LoadProject(csprojPath);

            // 关键：让引擎资源根目录指向“项目的 Content 目录”
            // 否则场景反序列化时会把相对路径错误地解析到 Editor 自己的 Content 下。
            var contentRoot = projectInfo.GameConfig?.ContentRootDirectory ?? "Content";
            var projectContentDir = Path.Combine(projectInfo.ProjectRoot, contentRoot);
            if (Directory.Exists(projectContentDir) && Engine.Content != null)
            {
                Engine.Content.RootDirectory = projectContentDir;
                System.Console.WriteLine($"[ProjectService] Content.RootDirectory => {Engine.Content.RootDirectory}");
            }
            else
            {
                System.Console.WriteLine($"[ProjectService] 警告：项目 Content 目录不存在: {projectContentDir}");
            }
            
            // 初始化场景管理器
            _sceneManager.Initialize();
            
            // 扫描场景
            _sceneManager.ScanScenes();
            
            // 编译并加载程序集
            var compileResult = _projectManager.CompileProject();
            if (compileResult.Success)
            {
                _projectManager.LoadProjectAssembly();
                
                // 获取已加载的程序集
                var loadedAssembly = _projectManager.GetLoadedAssembly();
                
                // 设置优先程序集，确保优先使用项目程序集中的类型
                _nodeTypeRegistry.SetPriorityAssembly(loadedAssembly);
                YamlSceneSerializer.SetPriorityAssembly(loadedAssembly);
                
                // 重新发现节点类型（包括项目中的自定义节点）
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
    
    /// <summary>
    /// 关闭当前项目
    /// </summary>
    public void CloseProject()
    {
        _projectManager.ClearProject();
        _nodeTypeRegistry.SetPriorityAssembly(null);
        YamlSceneSerializer.SetPriorityAssembly(null);
        _nodeTypeRegistry.MarkDirty();
        _nodeTypeRegistry.DiscoverNodeTypes();

        // 恢复默认 Content 根目录（Editor 自身）
        if (Engine.Content != null)
            Engine.Content.RootDirectory = "Content";
    }
    
    /// <summary>
    /// 重新编译项目
    /// </summary>
    public bool RebuildProject()
    {
        if (!_projectManager.HasProject)
        {
            System.Console.WriteLine("没有加载的项目");
            return false;
        }
        
        System.Console.WriteLine("开始重新加载程序集...");
        
        // 先清除优先程序集，确保不会扫描到旧程序集的类型
        _nodeTypeRegistry.SetPriorityAssembly(null);
        YamlSceneSerializer.SetPriorityAssembly(null);
        
        var result = _projectManager.ReloadAssembly();
        
        if (result)
        {
            // 获取新加载的程序集
            var loadedAssembly = _projectManager.GetLoadedAssembly();
            
            // 设置优先程序集，确保优先使用新程序集中的类型
            _nodeTypeRegistry.SetPriorityAssembly(loadedAssembly);
            
            // 重新发现节点类型（只扫描 Core 和项目程序集，忽略其他程序集）
            _nodeTypeRegistry.MarkDirty();
            _nodeTypeRegistry.DiscoverNodeTypes();
            
            // 更新序列化器的优先程序集，确保使用最新的类型
            YamlSceneSerializer.SetPriorityAssembly(loadedAssembly);
            
            System.Console.WriteLine("程序集重新加载成功");
        }
        else
        {
            System.Console.WriteLine("程序集重新加载失败，请查看上方的错误信息");
        }
        
        return result;
    }
}
