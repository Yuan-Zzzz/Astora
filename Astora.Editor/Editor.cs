using Astora.Core;
using Astora.Core.Nodes;
using Astora.Editor.Core;
using Astora.Editor.Services;
using Astora.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using System.IO;

namespace Astora.Editor
{
    public class Editor : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ImGuiRenderer _imGuiRenderer;
        
        // 服务层
        private readonly EditorConfig _config;
        private readonly ProjectService _projectService;
        private readonly EditorService _editorService;
        private readonly RenderService _renderService;
        
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
        private NotificationPanel _notificationPanel;
        
        public Editor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // 初始化配置和服务
            _config = new EditorConfig();
            _projectService = new ProjectService();
            _editorService = new EditorService(_projectService);
            _renderService = new RenderService();
            
            // 设置窗口大小
            _graphics.PreferredBackBufferWidth = _config.DefaultWindowWidth;
            _graphics.PreferredBackBufferHeight = _config.DefaultWindowHeight;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // 启用窗口缩放
            Window.AllowUserResizing = _config.AllowWindowResizing;
            
            // 设置最小窗口大小
            Window.ClientSizeChanged += OnClientSizeChanged;

            // 初始化 ImGui
            _imGuiRenderer = new ImGuiRenderer(this);
            
            // 添加中文字体支持
            var io = ImGui.GetIO();
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "msyh.ttc");
            if (File.Exists(fontPath))
            {
                // 添加微软雅黑字体，字体大小设置为 18，并包含中文字符范围
                io.Fonts.AddFontFromFileTTF(fontPath, 18.0f, null, io.Fonts.GetGlyphRangesChineseFull());
            }
            else
            {
                System.Console.WriteLine($"警告：未找到字体文件 {fontPath}，将使用默认字体");
            }
            
            _imGuiRenderer.RebuildFontAtlas();

            // 初始化面板
            _hierarchyPanel = new HierarchyPanel(_editorService.SceneTree, _projectService.NodeTypeRegistry);
            _inspectorPanel = new InspectorPanel(_projectService.ProjectManager, _imGuiRenderer);
            _sceneViewPanel = new SceneViewPanel(_editorService.SceneTree, _imGuiRenderer, this);
            _gameViewPanel = new GameViewPanel(_editorService.SceneTree, _imGuiRenderer, _projectService.ProjectManager);
            _projectPanel = new ProjectPanel(_projectService.ProjectManager, _projectService.SceneManager, this);
            _assetPanel = new AssetPanel(_projectService.ProjectManager, this);
            _projectLauncherPanel = new ProjectLauncherPanel(this);
            _createProjectDialog = new CreateProjectDialog(this);
            _menuBar = new MenuBar(this);
            _notificationPanel = new NotificationPanel(_editorService.State.NotificationManager);
        }

        protected override void LoadContent()
        {
            _spriteBatch = _renderService.GetSpriteBatch();
            Engine.Initialize(Content, _graphics);
            Engine.CurrentScene = _editorService.SceneTree;
        }
        
        protected override void Update(GameTime gameTime)
        {
            // 只有在播放模式下才更新场景
            if (_editorService.State.IsPlaying)
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
            GraphicsDevice.Clear(_config.BackgroundColor);
            
            // 渲染场景（编辑器视图）
            if (!_editorService.State.IsPlaying)
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
            
            if (!_editorService.State.IsProjectLoaded)
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
                var selectedNode = _editorService.GetSelectedNode();
                _hierarchyPanel.Render(ref selectedNode);
                _editorService.SetSelectedNode(selectedNode);
                _inspectorPanel.Render(selectedNode);
                
                // 根据播放模式显示不同的视图
                if (_editorService.State.IsPlaying)
                {
                    _gameViewPanel?.RenderUI();
                }
                else
                {
                    _sceneViewPanel.RenderUI();
                }
            }
            
            // 渲染通知面板（始终显示）
            _notificationPanel?.Render();
        }
        
        // 公共方法供面板调用 - 委托给服务
        public void SetSelectedNode(Node? node) => _editorService.SetSelectedNode(node);
        public Node? GetSelectedNode() => _editorService.GetSelectedNode();
        
        /// <summary>
        /// 设置播放状态
        /// </summary>
        public void SetPlaying(bool playing) => _editorService.SetPlaying(playing);
        
        /// <summary>
        /// 加载项目
        /// </summary>
        public bool LoadProject(string csprojPath)
        {
            var result = _projectService.LoadProject(csprojPath);
            if (result)
            {
                _editorService.State.IsProjectLoaded = true;
            }
            return result;
        }

        /// <summary>
        /// 关闭当前项目
        /// </summary>
        public void CloseProject()
        {
            _projectService.CloseProject();
            _editorService.OnProjectClosed();
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
            if (!_projectService.ProjectManager.HasProject)
            {
                System.Console.WriteLine("没有加载的项目");
                return false;
            }
            
            // 保存当前场景路径，以便重新加载
            var savedScenePath = _editorService.State.CurrentScenePath;
            
            var result = _projectService.RebuildProject();
            
            // 重新加载场景，使用新的类型创建节点实例
            if (result && !string.IsNullOrEmpty(savedScenePath) && File.Exists(savedScenePath))
            {
                System.Console.WriteLine($"重新加载场景: {savedScenePath}");
                _editorService.LoadScene(savedScenePath);
            }
            
            return result;
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(string path) => _editorService.LoadScene(path);
        
        /// <summary>
        /// 保存场景
        /// </summary>
        public void SaveScene(string? path = null) => _editorService.SaveScene(path);
        
        /// <summary>
        /// 创建新场景
        /// </summary>
        public void CreateNewScene(string sceneName) => _editorService.CreateNewScene(sceneName);
        
        /// <summary>
        /// 显示创建项目对话框
        /// </summary>
        public void ShowCreateProjectDialog()
        {
            _createProjectDialog?.Show();
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

        // 公共属性供面板访问
        public ProjectService ProjectService => _projectService;
        public EditorService EditorService => _editorService;
        public string? CurrentScenePath => _editorService.State.CurrentScenePath;
        public bool IsProjectLoaded => _editorService.State.IsProjectLoaded;
        
        // 为了向后兼容，保留这些属性
        public Project.ProjectManager ProjectManager => _projectService.ProjectManager;
        public Project.SceneManager SceneManager => _projectService.SceneManager;
    }
}
