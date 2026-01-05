using System;
using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
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
        private bool _isDragging = false;
        private bool _isPanning = false;
        private MXnaVector2 _lastMousePos;
        private MXnaVector2 _dragStartPos;
        private Node2D? _draggedNode;
        private float _rotateStartAngle;
        
        // Gizmo绘制
        private Texture2D? _whitePixelTexture;
        private const float GizmoHandleSize = 8f;
        private const float GizmoLineLength = 50f;
        
        public SceneViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer, Editor editor)
        {
            _sceneTree = sceneTree;
            _imGuiRenderer = imGuiRenderer;
            _editor = editor;
        }
        
        /// <summary>
        /// 创建白色像素纹理用于绘制Gizmo
        /// </summary>
        private Texture2D GetWhitePixelTexture()
        {
            if (_whitePixelTexture == null || _whitePixelTexture.IsDisposed)
            {
                _whitePixelTexture = new Texture2D(Engine.GraphicsDevice, 1, 1);
                _whitePixelTexture.SetData(new[] { Color.White });
            }
            return _whitePixelTexture;
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
        /// 查找被点击的节点
        /// </summary>
        private Node2D? FindNodeAtPosition(MXnaVector2 worldPos, Node? node)
        {
            if (node == null) return null;
            
            // 先检查子节点（从后往前，最后渲染的优先）
            Node2D? found = null;
            if (node.Children.Count > 0)
            {
                for (int i = node.Children.Count - 1; i >= 0; i--)
                {
                    found = FindNodeAtPosition(worldPos, node.Children[i]);
                    if (found != null) return found;
                }
            }
            
            // 检查当前节点
            if (node is Node2D node2d)
            {
                var bounds = GetNodeBounds(node2d);
                if (bounds.Contains(worldPos))
                {
                    return node2d;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取节点的边界框
        /// </summary>
        private RectangleF GetNodeBounds(Node2D node)
        {
            if (node is Sprite sprite)
            {
                var texture = sprite.Texture;
                if (texture != null)
                {
                    var size = new MXnaVector2(texture.Width * sprite.Scale.X, texture.Height * sprite.Scale.Y);
                    var pos = node.GlobalPosition;
                    return new RectangleF(
                        pos.X - sprite.Origin.X * sprite.Scale.X,
                        pos.Y - sprite.Origin.Y * sprite.Scale.Y,
                        size.X,
                        size.Y
                    );
                }
            }
            
            // 对于其他Node2D，使用默认大小
            var defaultSize = 32f;
            var defaultPos = node.GlobalPosition;
            return new RectangleF(
                defaultPos.X - defaultSize / 2f,
                defaultPos.Y - defaultSize / 2f,
                defaultSize,
                defaultSize
            );
        }
        
        /// <summary>
        /// 绘制Gizmo
        /// </summary>
        private void DrawGizmo(SpriteBatch spriteBatch, Node2D node)
        {
            var whitePixel = GetWhitePixelTexture();
            var worldPos = node.GlobalPosition;
            
            // 注意：Gizmo在世界坐标系中绘制，不需要转换为屏幕坐标
            // WorldToScreen只在RenderUI中用于鼠标交互时使用
            
            if (_currentTool == ToolMode.Move)
            {
                // 绘制移动Gizmo - 十字线
                var lineLength = GizmoLineLength / _cameraZoom;
                var handleSize = GizmoHandleSize / _cameraZoom;
                
                // 水平线
                DrawLine(spriteBatch, whitePixel,
                    new MXnaVector2(worldPos.X - lineLength, worldPos.Y),
                    new MXnaVector2(worldPos.X + lineLength, worldPos.Y),
                    Color.Red, 2f / _cameraZoom);
                
                // 垂直线
                DrawLine(spriteBatch, whitePixel,
                    new MXnaVector2(worldPos.X, worldPos.Y - lineLength),
                    new MXnaVector2(worldPos.X, worldPos.Y + lineLength),
                    Color.Green, 2f / _cameraZoom);
                
                // 中心点
                DrawCircle(spriteBatch, whitePixel, worldPos, handleSize, Color.White);
            }
            else if (_currentTool == ToolMode.Rotate)
            {
                // 绘制旋转Gizmo - 圆形
                var radius = GizmoLineLength / _cameraZoom;
                DrawCircle(spriteBatch, whitePixel, worldPos, radius, Color.Yellow, 2f / _cameraZoom);
                
                // 绘制旋转手柄
                var handleAngle = node.Rotation;
                if (node.Parent is Node2D parent2d)
                {
                    handleAngle += parent2d.Rotation;
                }
                var handlePos = worldPos + new MXnaVector2(
                    (float)Math.Cos(handleAngle) * radius,
                    (float)Math.Sin(handleAngle) * radius
                );
                DrawCircle(spriteBatch, whitePixel, handlePos, GizmoHandleSize / _cameraZoom, Color.Yellow);
            }
        }
        
        /// <summary>
        /// 绘制线条
        /// </summary>
        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, MXnaVector2 start, MXnaVector2 end, Color color, float thickness)
        {
            var direction = end - start;
            var length = direction.Length();
            var angle = (float)Math.Atan2(direction.Y, direction.X);
            
            spriteBatch.Draw(
                texture,
                start,
                null,
                color,
                angle,
                new MXnaVector2(0, 0.5f),
                new MXnaVector2(length, thickness),
                SpriteEffects.None,
                0f
            );
        }
        
        /// <summary>
        /// 绘制圆形（使用多个线段近似）
        /// </summary>
        private void DrawCircle(SpriteBatch spriteBatch, Texture2D texture, MXnaVector2 center, float radius, Color color, float thickness = 1f)
        {
            const int segments = 32;
            var angleStep = MathHelper.TwoPi / segments;
            
            for (int i = 0; i < segments; i++)
            {
                var angle1 = i * angleStep;
                var angle2 = (i + 1) * angleStep;
                
                var start = center + new MXnaVector2(
                    (float)Math.Cos(angle1) * radius,
                    (float)Math.Sin(angle1) * radius
                );
                var end = center + new MXnaVector2(
                    (float)Math.Cos(angle2) * radius,
                    (float)Math.Sin(angle2) * radius
                );
                
                DrawLine(spriteBatch, texture, start, end, color, thickness);
            }
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
        private RectangleF GetCameraViewportBounds(Camera2D camera)
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
            return new RectangleF(
                viewportTopLeft.X,
                viewportTopLeft.Y,
                viewportSize.X,
                viewportSize.Y
            );
        }
        
        /// <summary>
        /// 绘制矩形边框
        /// </summary>
        private void DrawRectangle(SpriteBatch spriteBatch, Texture2D texture, RectangleF bounds, Color color, float thickness)
        {
            var topLeft = new MXnaVector2(bounds.X, bounds.Y);
            var topRight = new MXnaVector2(bounds.X + bounds.Width, bounds.Y);
            var bottomLeft = new MXnaVector2(bounds.X, bounds.Y + bounds.Height);
            var bottomRight = new MXnaVector2(bounds.X + bounds.Width, bounds.Y + bounds.Height);
            
            // 绘制四条边
            DrawLine(spriteBatch, texture, topLeft, topRight, color, thickness);      // 上边
            DrawLine(spriteBatch, texture, topRight, bottomRight, color, thickness);  // 右边
            DrawLine(spriteBatch, texture, bottomRight, bottomLeft, color, thickness); // 下边
            DrawLine(spriteBatch, texture, bottomLeft, topLeft, color, thickness);     // 左边
        }
        
        /// <summary>
        /// 绘制所有相机的视口区域
        /// </summary>
        private void DrawCameraViewports(SpriteBatch spriteBatch)
        {
            var cameras = GetAllCameras();
            if (cameras.Count == 0) return;
            
            var whitePixel = GetWhitePixelTexture();
            var activeCamera = _sceneTree.ActiveCamera;
            
            foreach (var camera in cameras)
            {
                var bounds = GetCameraViewportBounds(camera);
                var isActive = camera == activeCamera;
                
                // ActiveCamera 用绿色，其他用黄色
                var color = isActive ? new Color(0, 255, 0, 200) : new Color(255, 255, 0, 150);
                var thickness = isActive ? 3f / _cameraZoom : 2f / _cameraZoom;
                
                // 绘制视口边框
                DrawRectangle(spriteBatch, whitePixel, bounds, color, thickness);
                
                // 在相机位置绘制一个小标记
                var cameraPos = camera.GlobalPosition;
                var markerSize = 8f / _cameraZoom;
                DrawCircle(spriteBatch, whitePixel, cameraPos, markerSize, color, thickness);
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
                DrawGizmo(spriteBatch, node2d);
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
                // 左键点击选择节点
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    var worldPos = ScreenToWorld(mousePos);
                    var clickedNode = FindNodeAtPosition(worldPos, _sceneTree.Root);
                    _editor.SetSelectedNode(clickedNode);
                    
                    // 开始拖拽
                    if (clickedNode is Node2D node2d && _currentTool != ToolMode.Select)
                    {
                        _isDragging = true;
                        _draggedNode = node2d;
                        _dragStartPos = worldPos;
                        
                        if (_currentTool == ToolMode.Rotate)
                        {
                            var nodeWorldPos = node2d.GlobalPosition;
                            var toMouse = worldPos - nodeWorldPos;
                            _rotateStartAngle = (float)Math.Atan2(toMouse.Y, toMouse.X);
                        }
                    }
                }
                
                // 处理拖拽
                if (_isDragging && _draggedNode != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    var currentWorldPos = ScreenToWorld(mousePos);
                    
                    if (_currentTool == ToolMode.Move)
                    {
                        var delta = currentWorldPos - _dragStartPos;
                        _draggedNode.Position += delta;
                        _dragStartPos = currentWorldPos;
                    }
                    else if (_currentTool == ToolMode.Rotate)
                    {
                        var nodeWorldPos = _draggedNode.GlobalPosition;
                        var toMouse = currentWorldPos - nodeWorldPos;
                        var currentAngle = (float)Math.Atan2(toMouse.Y, toMouse.X);
                        var angleDelta = currentAngle - _rotateStartAngle;
                        
                        // 如果父节点是Node2D，需要考虑父节点的旋转
                        if (_draggedNode.Parent is Node2D parent2d)
                        {
                            angleDelta -= parent2d.Rotation;
                        }
                        
                        _draggedNode.Rotation += angleDelta;
                        _rotateStartAngle = currentAngle;
                    }
                }
                
                // 结束拖拽
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    _isDragging = false;
                    _draggedNode = null;
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
    
    /// <summary>
    /// 简单的矩形结构用于碰撞检测
    /// </summary>
    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        
        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public bool Contains(MXnaVector2 point)
        {
            return point.X >= X && point.X <= X + Width &&
                   point.Y >= Y && point.Y <= Y + Height;
        }
    }
}
