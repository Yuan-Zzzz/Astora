using Astora.Core;
using Astora.Core.Scene;
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
        
        // UI 面板
        private HierarchyPanel _hierarchyPanel;
        private InspectorPanel _inspectorPanel;
        private SceneViewPanel _sceneViewPanel;
        private MenuBar _menuBar;
        
        // 编辑器状态
        private Node _selectedNode;
        private bool _isPlaying = false;
        private string _currentScenePath;
        
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
            _hierarchyPanel.Render(ref _selectedNode);
            _inspectorPanel.Render(_selectedNode);
            _sceneViewPanel.RenderUI();
        }
        
        // 公共方法供面板调用
        public void SetSelectedNode(Node node) => _selectedNode = node;
        public void SetPlaying(bool playing) => _isPlaying = playing;
        public void LoadScene(string path) { /* 实现场景加载 */ }
        public void SaveScene(string path) { /* 实现场景保存 */ }
    }
}