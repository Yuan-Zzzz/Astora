using Astora.Core;
using Astora.Core.Scene;
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
        
        // UI 面板
        private HierarchyPanel _hierarchyPanel;
        private InspectorPanel _inspectorPanel;
        private SceneViewPanel _sceneViewPanel;
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
            
            // 初始化项目管理器
            _projectManager = new ProjectManager();
            _sceneManager = new SceneManager(_projectManager);
        }

        protected override void Initialize()
        {
            base.Initialize();

            // 初始化 ImGui
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            // 初始化面板 - 传递 ImGuiRenderer 给 SceneViewPanel
            _hierarchyPanel = new HierarchyPanel(_sceneTree);
            _inspectorPanel = new InspectorPanel();
            _sceneViewPanel = new SceneViewPanel(_sceneTree, _imGuiRenderer);
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
                // 播放模式下的渲染（使用 Engine.Render）
                Engine.Render();
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
                _sceneViewPanel.RenderUI();
            }
        }
        
        // 公共方法供面板调用
        public void SetSelectedNode(Node? node) => _selectedNode = node;
        public void SetPlaying(bool playing) => _isPlaying = playing;
        
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
                return false;
            }
            
            return _projectManager.ReloadAssembly();
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

        public ProjectManager ProjectManager => _projectManager;
        public SceneManager SceneManager => _sceneManager;
        public string? CurrentScenePath => _currentScenePath;
        public bool IsProjectLoaded => _isProjectLoaded;
    }
}