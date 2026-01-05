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
        private MenuBar _menuBar;
        
        // 编辑器状态
        private Node? _selectedNode;
        private bool _isPlaying = false;
        private string? _currentScenePath;
        
        public Editor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // 设置窗口大小
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            
            _sceneTree = new SceneTree();
            Engine.CurretScene = _sceneTree;
            
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
            _menuBar = new MenuBar(this);
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Engine.Content = Content;
            Engine.GraphicsDevice = GraphicsDevice;
            Engine.SpriteBatch = _spriteBatch;
        }
        
        protected override void Update(GameTime gameTime)
        {
            // 只有在播放模式下才更新场景
            if (_isPlaying)
            {
                _sceneTree.Update(gameTime);
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
                // 播放模式下的渲染
                Matrix viewMatrix = Matrix.Identity;
                if (_sceneTree.ActiveCamera != null)
                {
                    viewMatrix = _sceneTree.ActiveCamera.GetViewMatrix();
                }
                
                _spriteBatch.Begin(
                    samplerState: SamplerState.PointClamp,
                    transformMatrix: viewMatrix
                );
                _sceneTree.Draw(_spriteBatch);
                _spriteBatch.End();
            }
            
            // 渲染 ImGui UI
            _imGuiRenderer.BeforeLayout(gameTime);
            RenderUI();
            _imGuiRenderer.AfterLayout();
            
            base.Draw(gameTime);
        }
        
        private void RenderUI()
        {
            // 菜单栏
            _menuBar.Render();
            
            // 主窗口布局（使用 ImGui 的 Docking）
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
            
            // 渲染各个面板
            _projectPanel?.Render();
            _hierarchyPanel.Render(ref _selectedNode);
            _inspectorPanel.Render(_selectedNode);
            _sceneViewPanel.RenderUI();
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
                
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"加载项目失败: {ex.Message}");
                return false;
            }
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
        
        public ProjectManager ProjectManager => _projectManager;
        public SceneManager SceneManager => _sceneManager;
        public string? CurrentScenePath => _currentScenePath;
    }
}