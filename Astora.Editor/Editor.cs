using Astora.Core;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Astora.Editor.Project;
using Astora.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace Astora.Editor
{
    public class Editor : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneTree _sceneTree;
        private ImGuiRenderer _imGuiRenderer;
        
        // 项目管理和场景管理
        private ProjectManager _projectManager;
        private SceneManager _sceneManager;
        
        // 节点类型注册表
        private NodeTypeRegistry _nodeTypeRegistry;
        
        // UI 面板
        private HierarchyPanel _hierarchyPanel;
        private InspectorPanel _inspectorPanel;
        private SceneViewPanel _sceneViewPanel;
        private GameViewPanel? _gameViewPanel;
        private ProjectPanel? _projectPanel;
        private AssetPanel? _assetPanel;
        private ProjectLauncherPanel? _projectLauncherPanel;
        private CreateProjectDialog? _createProjectDialog;
        private MenuBar _menuBar;
        
        // 编辑器状态
        private Node? _selectedNode;
        private bool _isPlaying = false;
        private string? _currentScenePath;
        private bool _isProjectLoaded = false;
        
        // 场景快照：用于在播放前保存场景状态，停止时恢复
        private string? _savedSceneSnapshotPath;
        
        public Editor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // 设置窗口大小
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            
            _sceneTree = new SceneTree();
            Engine.CurrentScene = _sceneTree;
            
            // 初始化节点类型注册表
            _nodeTypeRegistry = new NodeTypeRegistry();
            _nodeTypeRegistry.DiscoverNodeTypes();
            
            // 将节点类型注册表设置到序列化器，确保场景加载时使用最新的类型
            YamlSceneSerializer.SetNodeTypeRegistry(_nodeTypeRegistry);
            
            // 初始化项目管理器
            _projectManager = new ProjectManager();
            _sceneManager = new SceneManager(_projectManager);
        }

        protected override void Initialize()
        {
            base.Initialize();

            // 启用窗口缩放
            Window.AllowUserResizing = true;
            
            // 设置最小窗口大小
            Window.ClientSizeChanged += OnClientSizeChanged;

            // 初始化 ImGui
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            // 初始化面板 - 传递 ImGuiRenderer 给 SceneViewPanel
            _hierarchyPanel = new HierarchyPanel(_sceneTree, _nodeTypeRegistry);
            _inspectorPanel = new InspectorPanel(_projectManager, _imGuiRenderer);
            _sceneViewPanel = new SceneViewPanel(_sceneTree, _imGuiRenderer, this);
            _gameViewPanel = new GameViewPanel(_sceneTree, _imGuiRenderer, _projectManager);
            _projectPanel = new ProjectPanel(_projectManager, _sceneManager, this);
            _assetPanel = new AssetPanel(_projectManager, this);
            _projectLauncherPanel = new ProjectLauncherPanel(this);
            _createProjectDialog = new CreateProjectDialog(this);
            _menuBar = new MenuBar(this);
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Engine.Initialize(Content, GraphicsDevice, _spriteBatch);
            Engine.CurrentScene = _sceneTree;
        }
        
        protected override void Update(GameTime gameTime)
        {
            // 只有在播放模式下才更新场景
            if (_isPlaying)
            {
                Engine.Update(gameTime);
            }
            else
            {
                // 编辑器模式下的更新（如 Gizmo 交互）
                _sceneViewPanel.Update(gameTime);
            }
            
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(45, 45, 48)); // ImGui 风格的背景色
            
            // 渲染场景（编辑器视图）
            if (!_isPlaying)
            {
                _sceneViewPanel.Draw(_spriteBatch);
            }
            else
            {
                // 播放模式下的渲染 - Game窗口使用自己的RenderTarget
                _gameViewPanel?.Draw(_spriteBatch);
            }
            
            // 渲染 ImGui UI
            _imGuiRenderer.BeforeLayout(gameTime);
            RenderUI();
            _imGuiRenderer.AfterLayout();
            
            base.Draw(gameTime);
        }
        
        private void RenderUI()
        {
            // 菜单栏（始终显示）
            _menuBar.Render();
            
            if (!_isProjectLoaded)
            {
                // 显示项目启动器
                _projectLauncherPanel?.Render();
                _createProjectDialog?.Render();
            }
            else
            {
                // 显示正常的编辑器界面
                // 主窗口布局（使用 ImGui 的 Docking）
                var viewport = ImGui.GetMainViewport();
                
                // 创建 DockSpace（允许窗口停靠）
                ImGui.DockSpaceOverViewport(viewport.ID);
                
                // 渲染各个面板
                _projectPanel?.Render();
                _assetPanel?.Render();
                _hierarchyPanel.Render(ref _selectedNode);
                _inspectorPanel.Render(_selectedNode);
                
                // 根据播放模式显示不同的视图
                if (_isPlaying)
                {
                    _gameViewPanel?.RenderUI();
                }
                else
                {
                    _sceneViewPanel.RenderUI();
                }
            }
        }
        
        // 公共方法供面板调用
        public void SetSelectedNode(Node? node) => _selectedNode = node;
        public Node? GetSelectedNode() => _selectedNode;
        
        /// <summary>
        /// 设置播放状态
        /// </summary>
        public void SetPlaying(bool playing)
        {
            if (playing && !_isPlaying)
            {
                // 开始播放：保存当前场景状态
                SaveSceneSnapshot();
            }
            else if (!playing && _isPlaying)
            {
                // 停止播放：恢复场景到初始状态
                RestoreSceneSnapshot();
            }
            
            _isPlaying = playing;
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
                _savedSceneSnapshotPath = Path.Combine(tempDir, tempFileName);
                
                // 保存场景到临时文件
                Engine.Serializer.Save(_sceneTree.Root, _savedSceneSnapshotPath);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"保存场景快照失败: {ex.Message}");
                _savedSceneSnapshotPath = null;
            }
        }
        
        /// <summary>
        /// 恢复场景快照（停止时调用）
        /// </summary>
        private void RestoreSceneSnapshot()
        {
            if (string.IsNullOrEmpty(_savedSceneSnapshotPath) || !File.Exists(_savedSceneSnapshotPath))
            {
                // 没有快照或文件不存在，无需恢复
                return;
            }
            
            try
            {
                // 从临时文件加载场景
                var restoredScene = Engine.Serializer.Load(_savedSceneSnapshotPath);
                
                // 恢复场景
                _sceneTree.ChangeScene(restoredScene);
                
                // 清除选中的节点（因为节点对象已改变）
                _selectedNode = null;
                
                // 清理临时文件
                try
                {
                    File.Delete(_savedSceneSnapshotPath);
                }
                catch
                {
                    // 忽略删除失败的错误
                }
                
                _savedSceneSnapshotPath = null;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"恢复场景快照失败: {ex.Message}");
                // 清理临时文件
                try
                {
                    if (File.Exists(_savedSceneSnapshotPath))
                    {
                        File.Delete(_savedSceneSnapshotPath);
                    }
                }
                catch
                {
                    // 忽略删除失败的错误
                }
                _savedSceneSnapshotPath = null;
            }
        }
        
        /// <summary>
        /// 加载项目
        /// </summary>
        public bool LoadProject(string csprojPath)
        {
            try
            {
                var projectInfo = _projectManager.LoadProject(csprojPath);
                
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
                    
                    // 更新序列化器的节点类型注册表
                    YamlSceneSerializer.SetNodeTypeRegistry(_nodeTypeRegistry);
                }
                else
                {
                    System.Console.WriteLine($"编译警告: {compileResult.ErrorMessage}");
                }
                
                // 标记项目已加载
                _isProjectLoaded = true;
                
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
            // 如果正在播放，先停止
            if (_isPlaying)
            {
                SetPlaying(false);
            }
            
            // 清理临时快照文件
            if (!string.IsNullOrEmpty(_savedSceneSnapshotPath) && File.Exists(_savedSceneSnapshotPath))
            {
                try
                {
                    File.Delete(_savedSceneSnapshotPath);
                }
                catch
                {
                    // 忽略删除失败的错误
                }
                _savedSceneSnapshotPath = null;
            }
            
            _projectManager.ClearProject();
            _sceneTree.ChangeScene(null);
            _currentScenePath = null;
            _selectedNode = null;
            _isPlaying = false;
            _isProjectLoaded = false;
        }

        /// <summary>
        /// 显示打开项目对话框
        /// </summary>
        public void ShowOpenProjectDialog()
        {
            _menuBar.ShowOpenProjectDialog();
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
            
            // 保存当前场景路径，以便重新加载
            var savedScenePath = _currentScenePath;
            
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
                
                // 更新序列化器的节点类型注册表和优先程序集，确保使用最新的类型
                YamlSceneSerializer.SetNodeTypeRegistry(_nodeTypeRegistry);
                YamlSceneSerializer.SetPriorityAssembly(loadedAssembly);
                
                // 重新加载场景，使用新的类型创建节点实例
                if (!string.IsNullOrEmpty(savedScenePath) && File.Exists(savedScenePath))
                {
                    System.Console.WriteLine($"重新加载场景: {savedScenePath}");
                    LoadScene(savedScenePath);
                }
                
                System.Console.WriteLine("程序集重新加载成功");
            }
            else
            {
                System.Console.WriteLine("程序集重新加载失败，请查看上方的错误信息");
            }
            
            return result;
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(string path)
        {
            var scene = _sceneManager.LoadScene(path);
            if (scene != null)
            {
                _sceneTree.ChangeScene(scene);
                _currentScenePath = path;
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
            
            var savePath = path ?? _currentScenePath;
            if (string.IsNullOrEmpty(savePath))
            {
                // 如果没有路径，使用当前场景名称
                savePath = _sceneManager.GetScenePath(_sceneTree.Root.Name);
            }
            
            _sceneManager.SaveScene(savePath, _sceneTree.Root);
            _currentScenePath = savePath;
        }
        
        /// <summary>
        /// 创建新场景
        /// </summary>
        public void CreateNewScene(string sceneName)
        {
            var scenePath = _sceneManager.CreateNewScene(sceneName);
            LoadScene(scenePath);
        }
        
        /// <summary>
        /// 显示创建项目对话框
        /// </summary>
        public void ShowCreateProjectDialog()
        {
            _createProjectDialog?.Show();
        }

        /// <summary>
        /// 设置默认布局
        /// </summary>
        private void SetupDefaultLayout(uint dockSpaceId, ImGuiViewportPtr viewport)
        {
            // ImGui.NET 的 DockBuilder API 可能不可用或不同
            // 使用 DockSpaceOverViewport 让用户手动拖拽窗口来设置布局
            // ImGui 会自动保存布局到 imgui.ini 文件
            
            // 首次加载时，窗口会出现在默认位置
            // 用户可以通过拖拽窗口标题栏来停靠窗口
            // 布局会自动保存，下次启动时会恢复
        }

        /// <summary>
        /// 处理窗口大小改变事件
        /// </summary>
        private void OnClientSizeChanged(object? sender, EventArgs e)
        {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                _graphics.ApplyChanges();
            }
        }

        public ProjectManager ProjectManager => _projectManager;
        public SceneManager SceneManager => _sceneManager;
        public string? CurrentScenePath => _currentScenePath;
        public bool IsProjectLoaded => _isProjectLoaded;
    }
}