using System;
using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
using MXnaVector2 = Microsoft.Xna.Framework.Vector2;
using MXnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 工具模式枚举
    /// </summary>
    public enum ToolMode
    {
        Select,
        Move,
        Rotate
    }

    public class SceneViewPanel
    {
        private SceneTree _sceneTree;
        private RenderTarget2D _sceneRenderTarget;
        private IntPtr _renderTargetTextureId;
        private ImGuiRenderer _imGuiRenderer;
        private Editor _editor;
        
        // 相机控制
        private MXnaVector2 _cameraPosition;
        private float _cameraZoom = 1.0f;
        
        // 工具模式
        private ToolMode _currentTool = ToolMode.Select;
        
        // 输入状态
        private bool _isPanning = false;
        private MXnaVector2 _lastMousePos;
        
        // Gizmo 和工具
        private GizmoRenderer _gizmoRenderer;
        private SelectionTool _selectionTool;
        private MoveTool _moveTool;
        private RotateTool _rotateTool;
        
        public SceneViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer, Editor editor)
        {
            _sceneTree = sceneTree;
            _imGuiRenderer = imGuiRenderer;
            _editor = editor;
            
            // 初始化 Gizmo 渲染器和工具
            _gizmoRenderer = new GizmoRenderer();
            _selectionTool = new SelectionTool(sceneTree);
            _moveTool = new MoveTool();
            _rotateTool = new RotateTool();
        }
        
        /// <summary>
        /// 将ImGui窗口坐标转换为世界坐标
        /// 注意：此方法只能在ImGui窗口打开时调用（即在RenderUI中，ImGui.Begin()之后）
        /// </summary>
        private MXnaVector2 ScreenToWorld(Vector2 screenPos)
        {
            // 获取窗口内容区域的位置
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            
            // 计算相对于内容区域的坐标
            var localPos = new MXnaVector2(
                screenPos.X - cursorScreenPos.X,
                screenPos.Y - cursorScreenPos.Y
            );
            
            // 应用相机变换的逆变换
            Matrix viewMatrix = Matrix.CreateTranslation(new MXnaVector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            Matrix.Invert(ref viewMatrix, out var invViewMatrix);
            
            return MXnaVector2.Transform(localPos, invViewMatrix);
        }
        
        /// <summary>
        /// 将世界坐标转换为ImGui窗口坐标
        /// 注意：此方法只能在ImGui窗口打开时调用（即在RenderUI中，ImGui.Begin()之后）
        /// </summary>
        private Vector2 WorldToScreen(MXnaVector2 worldPos)
        {
            // 应用相机变换
            Matrix viewMatrix = Matrix.CreateTranslation(new MXnaVector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            
            var screenPos = MXnaVector2.Transform(worldPos, viewMatrix);
            
            // 转换为窗口坐标
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            
            return new Vector2(
                screenPos.X + cursorScreenPos.X,
                screenPos.Y + cursorScreenPos.Y
            );
        }
        
        /// <summary>
        /// 获取当前工具
        /// </summary>
        private ITool GetCurrentTool()
        {
            return _currentTool switch
            {
                ToolMode.Select => _selectionTool,
                ToolMode.Move => _moveTool,
                ToolMode.Rotate => _rotateTool,
                _ => _selectionTool
            };
        }
        
        /// <summary>
        /// 递归查找场景中所有的 Camera2D 节点
        /// </summary>
        private List<Camera2D> GetAllCameras()
        {
            var cameras = new List<Camera2D>();
            FindCameras(_sceneTree.Root, cameras);
            return cameras;
        }
        
        /// <summary>
        /// 递归查找相机节点
        /// </summary>
        private void FindCameras(Node? node, List<Camera2D> cameras)
        {
            if (node == null) return;
            
            if (node is Camera2D camera)
            {
                cameras.Add(camera);
            }
            
            foreach (var child in node.Children)
            {
                FindCameras(child, cameras);
            }
        }
        
        /// <summary>
        /// 计算相机的世界坐标视口边界
        /// </summary>
        private Tools.RectangleF GetCameraViewportBounds(Camera2D camera)
        {
            // 获取设计分辨率（从项目配置）或使用默认值
            int designWidth = 1920;
            int designHeight = 1080;
            
            var projectManager = _editor.ProjectManager;
            if (projectManager?.CurrentProject?.GameConfig != null)
            {
                var config = projectManager.CurrentProject.GameConfig;
                designWidth = config.DesignWidth;
                designHeight = config.DesignHeight;
            }
            else if (Engine.GraphicsDevice != null)
            {
                // 如果没有项目配置，使用 GraphicsDevice 的视口大小
                var viewport = Engine.GraphicsDevice.Viewport;
                designWidth = viewport.Width;
                designHeight = viewport.Height;
            }
            
            // 计算视口大小（考虑相机的 Zoom）
            var viewportSize = new MXnaVector2(designWidth / camera.Zoom, designHeight / camera.Zoom);
            
            // 相机的世界位置
            var cameraWorldPos = camera.GlobalPosition;
            
            // 根据 Camera2D.GetViewMatrix() 的变换逻辑：
            // 1. Translate(-Position) - 将世界坐标转换为以相机位置为原点的坐标系
            // 2. Rotate(-Rotation)
            // 3. Scale(Zoom)
            // 4. Translate(Origin) - 将结果平移到屏幕上的Origin位置
            // 
            // 这意味着相机的世界位置会被映射到屏幕上的Origin位置
            // 屏幕左上角(0,0)对应的世界坐标 = Position - Origin / Zoom
            // 屏幕中心(Origin)对应的世界坐标 = Position
            
            // 计算视口左上角的世界坐标
            var viewportTopLeft = cameraWorldPos - camera.Origin / camera.Zoom;
            
            // 返回边界矩形
            return new Tools.RectangleF(
                viewportTopLeft.X,
                viewportTopLeft.Y,
                viewportSize.X,
                viewportSize.Y
            );
        }
        
        /// <summary>
        /// 绘制所有相机的视口区域
        /// </summary>
        private void DrawCameraViewports(SpriteBatch spriteBatch)
        {
            var cameras = GetAllCameras();
            if (cameras.Count == 0) return;
            
            var activeCamera = _sceneTree.ActiveCamera;
            
            foreach (var camera in cameras)
            {
                var bounds = GetCameraViewportBounds(camera);
                var isActive = camera == activeCamera;
                
                // ActiveCamera 用绿色，其他用黄色
                var color = isActive ? new Color(0, 255, 0, 200) : new Color(255, 255, 0, 150);
                var thickness = isActive ? 3f / _cameraZoom : 2f / _cameraZoom;
                
                // 绘制视口边框
                _gizmoRenderer.DrawRectangle(spriteBatch, bounds, color, thickness);
                
                // 在相机位置绘制一个小标记
                var cameraPos = camera.GlobalPosition;
                var markerSize = 8f / _cameraZoom;
                _gizmoRenderer.DrawCircle(spriteBatch, cameraPos, markerSize, color, thickness);
            }
        }
        
        public void Update(GameTime gameTime)
        {
            // 处理输入和交互逻辑在RenderUI中处理
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_sceneRenderTarget == null) return;
            
            // 设置RenderTarget
            Engine.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            Engine.GraphicsDevice.Clear(Color.Black);
            
            // 渲染场景到RenderTarget
            Matrix viewMatrix = Matrix.CreateTranslation(new MXnaVector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: viewMatrix
            );
            
            _sceneTree.Draw(spriteBatch);
            
            // 绘制选中节点的Gizmo
            var selectedNode = _editor.GetSelectedNode();
            if (selectedNode is Node2D node2d)
            {
                var currentTool = GetCurrentTool();
                currentTool.DrawGizmo(spriteBatch, _gizmoRenderer, node2d, _cameraZoom);
            }
            
            // 绘制所有相机的视口区域
            DrawCameraViewports(spriteBatch);
            
            spriteBatch.End();
            
            // 恢复默认RenderTarget
            Engine.GraphicsDevice.SetRenderTarget(null);
        }
        
        public void RenderUI()
        {
            ImGui.Begin("Scene View");
            
            // 工具栏
            if (ImGui.Button("Select"))
            {
                _currentTool = ToolMode.Select;
            }
            ImGui.SameLine();
            if (ImGui.Button("Move"))
            {
                _currentTool = ToolMode.Move;
            }
            ImGui.SameLine();
            if (ImGui.Button("Rotate"))
            {
                _currentTool = ToolMode.Rotate;
            }
            
            // 显示当前工具
            ImGui.SameLine();
            ImGui.Text($"Tool: {_currentTool}");
            
            // 检查窗口是否被悬停
            bool isWindowHovered = ImGui.IsWindowHovered();
            bool isContentHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && 
                                    ImGui.IsWindowHovered(ImGuiHoveredFlags.RootWindow);
            
            // 获取鼠标位置
            var mousePos = ImGui.GetMousePos();
            var mouseDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left);
            
            // 处理相机控制（中键拖拽平移，滚轮缩放）
            if (isWindowHovered)
            {
                // 中键拖拽平移
                if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                {
                    if (!_isPanning)
                    {
                        _isPanning = true;
                        _lastMousePos = new MXnaVector2(mousePos.X, mousePos.Y);
                    }
                    else
                    {
                        var currentMousePos = new MXnaVector2(mousePos.X, mousePos.Y);
                        var delta = (currentMousePos - _lastMousePos) / _cameraZoom;
                        _cameraPosition += delta;
                        _lastMousePos = currentMousePos;
                    }
                }
                else
                {
                    _isPanning = false;
                }
                
                // 滚轮缩放
                var scrollDelta = ImGui.GetIO().MouseWheel;
                if (scrollDelta != 0)
                {
                    var zoomFactor = 1.1f;
                    if (scrollDelta > 0)
                    {
                        _cameraZoom *= zoomFactor;
                    }
                    else
                    {
                        _cameraZoom /= zoomFactor;
                    }
                    _cameraZoom = MathHelper.Clamp(_cameraZoom, 0.1f, 10f);
                }
            }
            else
            {
                _isPanning = false;
            }
            
            // 处理节点选择和操作
            if (isWindowHovered && !_isPanning)
            {
                var worldPos = ScreenToWorld(mousePos);
                var selectedNode = _editor.GetSelectedNode() as Node2D;
                var currentTool = GetCurrentTool();
                
                // 左键点击选择节点
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    // 使用选择工具查找节点
                    var clickedNode = _selectionTool.FindNodeAtPosition(worldPos);
                    _editor.SetSelectedNode(clickedNode);
                    
                    // 如果点击了节点且不是选择工具，通知当前工具开始操作
                    if (clickedNode is Node2D node2d && _currentTool != ToolMode.Select)
                    {
                        currentTool.OnMouseDown(worldPos, node2d);
                    }
                }
                
                // 处理拖拽
                if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    currentTool.OnMouseDrag(worldPos, selectedNode);
                }
                
                // 结束拖拽
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    currentTool.OnMouseUp(worldPos, selectedNode);
                }
            }
            
            // 渲染场景视图
            var viewportSize = ImGui.GetContentRegionAvail();
            if (viewportSize.X > 0 && viewportSize.Y > 0)
            {
                // 创建或调整RenderTarget大小
                if (_sceneRenderTarget == null || 
                    _sceneRenderTarget.Width != (int)viewportSize.X || 
                    _sceneRenderTarget.Height != (int)viewportSize.Y)
                {
                    // 如果纹理ID存在，先解绑
                    if (_renderTargetTextureId != IntPtr.Zero)
                    {
                        _imGuiRenderer.UnbindTexture(_renderTargetTextureId);
                    }
                    
                    _sceneRenderTarget?.Dispose();
                    _sceneRenderTarget = new RenderTarget2D(
                        Engine.GraphicsDevice,
                        (int)viewportSize.X,
                        (int)viewportSize.Y
                    );
                    
                    // 绑定RenderTarget到ImGui
                    _renderTargetTextureId = _imGuiRenderer.BindTexture(_sceneRenderTarget);
                }
                
                // 显示渲染结果
                ImGui.Image(
                    _renderTargetTextureId,
                    viewportSize,
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One
                );
            }
            
            ImGui.End();
        }
    }
}
