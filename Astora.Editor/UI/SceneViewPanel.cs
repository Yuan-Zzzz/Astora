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
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

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
        private XnaVector2 _cameraPosition;
        private float _cameraZoom = 1.0f;
        
        // 工具模式
        private ToolMode _currentTool = ToolMode.Select;
        
        // 输入状态
        private bool _isPanning = false;
        private XnaVector2 _lastMousePos;
        
        // Gizmo 和工具
        private GizmoRenderer _gizmoRenderer;
        private SelectionTool _selectionTool;
        private MoveTool _moveTool;
        private RotateTool _rotateTool;
        
        // 最小线宽常量，确保线条始终可见
        private const float MinLineThickness = 1f;
        
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
        private XnaVector2 ScreenToWorld(Vector2 screenPos)
        {
            // 获取窗口内容区域的位置
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            
            // 计算相对于内容区域的坐标
            var localPos = new XnaVector2(
                screenPos.X - cursorScreenPos.X,
                screenPos.Y - cursorScreenPos.Y
            );
            
            // 应用相机变换的逆变换
            Matrix viewMatrix = Matrix.CreateTranslation(new XnaVector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            Matrix.Invert(ref viewMatrix, out var invViewMatrix);
            
            return XnaVector2.Transform(localPos, invViewMatrix);
        }
        
        /// <summary>
        /// 将世界坐标转换为ImGui窗口坐标
        /// 注意：此方法只能在ImGui窗口打开时调用（即在RenderUI中，ImGui.Begin()之后）
        /// </summary>
        private Vector2 WorldToScreen(XnaVector2 worldPos)
        {
            // 应用相机变换
            Matrix viewMatrix = Matrix.CreateTranslation(new XnaVector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            
            var screenPos = XnaVector2.Transform(worldPos, viewMatrix);
            
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
            var viewportSize = new XnaVector2(designWidth / camera.Zoom, designHeight / camera.Zoom);
            
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
        
        /// <summary>
        /// 绘制辅助网格
        /// </summary>
        private void DrawGrid(SpriteBatch spriteBatch)
        {
            if (_sceneRenderTarget == null) return;
            
            // 计算当前视口在世界坐标系中的范围
            // viewMatrix = Translate(-_cameraPosition) * Scale(_cameraZoom)
            // 所以：screenPos = (worldPos - _cameraPosition) * _cameraZoom
            // 反过来：worldPos = screenPos / _cameraZoom + _cameraPosition
            var viewportWidth = _sceneRenderTarget.Width;
            var viewportHeight = _sceneRenderTarget.Height;
            
            // 屏幕(0,0)对应的世界坐标
            var worldTopLeft = new XnaVector2(
                0 / _cameraZoom + _cameraPosition.X,
                0 / _cameraZoom + _cameraPosition.Y
            );
            // 屏幕(viewportWidth, viewportHeight)对应的世界坐标
            var worldBottomRight = new XnaVector2(
                viewportWidth / _cameraZoom + _cameraPosition.X,
                viewportHeight / _cameraZoom + _cameraPosition.Y
            );
            
            // 确保范围正确（考虑相机位置可能为负）
            var worldMinX = Math.Min(worldTopLeft.X, worldBottomRight.X);
            var worldMaxX = Math.Max(worldTopLeft.X, worldBottomRight.X);
            var worldMinY = Math.Min(worldTopLeft.Y, worldBottomRight.Y);
            var worldMaxY = Math.Max(worldTopLeft.Y, worldBottomRight.Y);
            
            // 扩展范围以确保网格覆盖整个视口
            var padding = 100f;
            worldMinX -= padding;
            worldMaxX += padding;
            worldMinY -= padding;
            worldMaxY += padding;
            
            // 网格间距
            const float minorGridSpacing = 50f;
            const float majorGridSpacing = 100f;
            
            // 根据缩放级别决定是否绘制次网格
            bool drawMinorGrid = _cameraZoom > 0.3f;
            
            // 计算网格线的起始位置（对齐到网格）
            var startX = (float)(Math.Floor(worldMinX / minorGridSpacing) * minorGridSpacing);
            var startY = (float)(Math.Floor(worldMinY / minorGridSpacing) * minorGridSpacing);
            
            // 绘制次网格线（50像素间距）
            if (drawMinorGrid)
            {
                var minorColor = new Color(128, 128, 128, 100);
                var minorThickness = Math.Max(1f / _cameraZoom, MinLineThickness);
                
                // 垂直线
                for (float x = startX; x <= worldMaxX; x += minorGridSpacing)
                {
                    // 跳过主网格线位置
                    if (Math.Abs(x % majorGridSpacing) < 0.1f)
                        continue;
                    
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(x, worldMinY),
                        new XnaVector2(x, worldMaxY),
                        minorColor,
                        minorThickness
                    );
                }
                
                // 水平线
                for (float y = startY; y <= worldMaxY; y += minorGridSpacing)
                {
                    // 跳过主网格线位置
                    if (Math.Abs(y % majorGridSpacing) < 0.1f)
                        continue;
                    
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(worldMinX, y),
                        new XnaVector2(worldMaxX, y),
                        minorColor,
                        minorThickness
                    );
                }
            }
            
            // 绘制主网格线（100像素间距）
            var majorColor = new Color(100, 100, 100, 150);
            var majorThickness = Math.Max(1f / _cameraZoom, MinLineThickness);
            
            var majorStartX = (float)(Math.Floor(worldMinX / majorGridSpacing) * majorGridSpacing);
            var majorStartY = (float)(Math.Floor(worldMinY / majorGridSpacing) * majorGridSpacing);
            
            // 垂直线
            for (float x = majorStartX; x <= worldMaxX; x += majorGridSpacing)
            {
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(x, worldMinY),
                    new XnaVector2(x, worldMaxY),
                    majorColor,
                    majorThickness
                );
            }
            
            // 水平线
            for (float y = majorStartY; y <= worldMaxY; y += majorGridSpacing)
            {
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(worldMinX, y),
                    new XnaVector2(worldMaxX, y),
                    majorColor,
                    majorThickness
                );
            }
        }
        
        /// <summary>
        /// 绘制坐标轴（X轴红色，Y轴绿色，原点处加粗）
        /// </summary>
        private void DrawCoordinateAxes(SpriteBatch spriteBatch)
        {
            if (_sceneRenderTarget == null) return;
            
            // 计算当前视口在世界坐标系中的范围
            var viewportWidth = _sceneRenderTarget.Width;
            var viewportHeight = _sceneRenderTarget.Height;
            
            // 屏幕(0,0)对应的世界坐标
            var worldTopLeft = new XnaVector2(
                0 / _cameraZoom + _cameraPosition.X,
                0 / _cameraZoom + _cameraPosition.Y
            );
            // 屏幕(viewportWidth, viewportHeight)对应的世界坐标
            var worldBottomRight = new XnaVector2(
                viewportWidth / _cameraZoom + _cameraPosition.X,
                viewportHeight / _cameraZoom + _cameraPosition.Y
            );
            
            var worldMinX = Math.Min(worldTopLeft.X, worldBottomRight.X);
            var worldMaxX = Math.Max(worldTopLeft.X, worldBottomRight.X);
            var worldMinY = Math.Min(worldTopLeft.Y, worldBottomRight.Y);
            var worldMaxY = Math.Max(worldTopLeft.Y, worldBottomRight.Y);
            
            // 扩展范围以确保坐标轴覆盖整个视口
            var padding = 100f;
            worldMinX -= padding;
            worldMaxX += padding;
            worldMinY -= padding;
            worldMaxY += padding;
            
            // 坐标轴线宽
            var axisThickness = Math.Max(1.5f / _cameraZoom, MinLineThickness);
            var originThickness = Math.Max(4f / _cameraZoom, MinLineThickness * 2f); // 原点处加粗，至少是最小线宽的2倍
            
            // 绘制X轴（红色）
            if (worldMinY <= 0 && worldMaxY >= 0)
            {
                // 检查原点是否在视口内
                bool originVisible = (worldMinX <= 0 && worldMaxX >= 0);
                
                if (originVisible)
                {
                    // 原点左侧
                    if (worldMinX < 0)
                    {
                        _gizmoRenderer.DrawLine(
                            spriteBatch,
                            new XnaVector2(worldMinX, 0),
                            new XnaVector2(0, 0),
                            Color.Red,
                            axisThickness
                        );
                    }
                    
                    // 原点处加粗
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(-5f / _cameraZoom, 0),
                        new XnaVector2(5f / _cameraZoom, 0),
                        Color.Red,
                        originThickness
                    );
                    
                    // 原点右侧
                    if (worldMaxX > 0)
                    {
                        _gizmoRenderer.DrawLine(
                            spriteBatch,
                            new XnaVector2(0, 0),
                            new XnaVector2(worldMaxX, 0),
                            Color.Red,
                            axisThickness
                        );
                    }
                }
                else
                {
                    // 原点不在视口内，绘制整条线
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(worldMinX, 0),
                        new XnaVector2(worldMaxX, 0),
                        Color.Red,
                        axisThickness
                    );
                }
            }
            
            // 绘制Y轴（绿色）
            if (worldMinX <= 0 && worldMaxX >= 0)
            {
                // 检查原点是否在视口内
                bool originVisible = (worldMinY <= 0 && worldMaxY >= 0);
                
                if (originVisible)
                {
                    // 原点下方
                    if (worldMinY < 0)
                    {
                        _gizmoRenderer.DrawLine(
                            spriteBatch,
                            new XnaVector2(0, worldMinY),
                            new XnaVector2(0, 0),
                            Color.Green,
                            axisThickness
                        );
                    }
                    
                    // 原点处加粗
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(0, -5f / _cameraZoom),
                        new XnaVector2(0, 5f / _cameraZoom),
                        Color.Green,
                        originThickness
                    );
                    
                    // 原点上方的Y轴
                    if (worldMaxY > 0)
                    {
                        _gizmoRenderer.DrawLine(
                            spriteBatch,
                            new XnaVector2(0, 0),
                            new XnaVector2(0, worldMaxY),
                            Color.Green,
                            axisThickness
                        );
                    }
                }
                else
                {
                    // 原点不在视口内，绘制整条线
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(0, worldMinY),
                        new XnaVector2(0, worldMaxY),
                        Color.Green,
                        axisThickness
                    );
                }
            }
        }
        
        /// <summary>
        /// 绘制标尺（在ImGui中绘制，显示世界坐标像素值）
        /// </summary>
        private void DrawRulers()
        {
            if (_sceneRenderTarget == null) return;
            
            var drawList = ImGui.GetWindowDrawList();
            // 获取场景视图的起始位置（与Image使用相同的位置）
            var viewportStartPos = ImGui.GetCursorScreenPos();
            var viewportSize = ImGui.GetContentRegionAvail();
            
            // 标尺尺寸
            const float rulerSize = 25f;
            
            // 计算当前视口在世界坐标系中的范围
            var viewportWidth = _sceneRenderTarget.Width;
            var viewportHeight = _sceneRenderTarget.Height;
            
            // 屏幕(0,0)对应的世界坐标
            var worldTopLeft = new XnaVector2(
                0 / _cameraZoom + _cameraPosition.X,
                0 / _cameraZoom + _cameraPosition.Y
            );
            // 屏幕(viewportWidth, viewportHeight)对应的世界坐标
            var worldBottomRight = new XnaVector2(
                viewportWidth / _cameraZoom + _cameraPosition.X,
                viewportHeight / _cameraZoom + _cameraPosition.Y
            );
            
            var worldMinX = Math.Min(worldTopLeft.X, worldBottomRight.X);
            var worldMaxX = Math.Max(worldTopLeft.X, worldBottomRight.X);
            var worldMinY = Math.Min(worldTopLeft.Y, worldBottomRight.Y);
            var worldMaxY = Math.Max(worldTopLeft.Y, worldBottomRight.Y);
            
            // 根据缩放级别确定刻度间距
            float majorTickSpacing = 100f;
            float minorTickSpacing = 50f;
            
            // 如果缩放太小，增大刻度间距
            if (_cameraZoom < 0.5f)
            {
                majorTickSpacing = 200f;
                minorTickSpacing = 100f;
            }
            else if (_cameraZoom < 0.2f)
            {
                majorTickSpacing = 500f;
                minorTickSpacing = 250f;
            }
            
            // 标尺背景颜色
            var rulerBgColor = ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.2f, 0.8f));
            var tickColor = ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            var textColor = ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            
            // 绘制顶部水平标尺（X轴）
            var topRulerRect = new System.Numerics.Vector2(
                viewportStartPos.X,
                viewportStartPos.Y - rulerSize
            );
            var topRulerRectMax = new System.Numerics.Vector2(
                viewportStartPos.X + viewportSize.X,
                viewportStartPos.Y
            );
            drawList.AddRectFilled(topRulerRect, topRulerRectMax, rulerBgColor);
            
            // 计算需要显示的刻度范围
            var startX = (float)(Math.Floor(worldMinX / minorTickSpacing) * minorTickSpacing);
            var endX = (float)(Math.Ceiling(worldMaxX / minorTickSpacing) * minorTickSpacing);
            
            for (float worldX = startX; worldX <= endX; worldX += minorTickSpacing)
            {
                // 将世界坐标转换为屏幕坐标（使用与WorldToScreen相同的逻辑）
                var worldPos = new XnaVector2(worldX, 0);
                var screenPos = WorldToScreen(worldPos);
                var screenX = screenPos.X;
                
                if (screenX < viewportStartPos.X || screenX > viewportStartPos.X + viewportSize.X)
                    continue;
                
                bool isMajor = Math.Abs(worldX % majorTickSpacing) < 0.1f;
                float tickLength = isMajor ? 12f : 6f;
                
                // 绘制刻度线
                var tickStart = new System.Numerics.Vector2(screenX, viewportStartPos.Y - tickLength);
                var tickEnd = new System.Numerics.Vector2(screenX, viewportStartPos.Y);
                drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
                
                // 绘制主要刻度的文字标签
                if (isMajor)
                {
                    var labelText = ((int)worldX).ToString();
                    var textSize = ImGui.CalcTextSize(labelText);
                    var textPos = new System.Numerics.Vector2(
                        screenX - textSize.X * 0.5f,
                        viewportStartPos.Y - rulerSize + (rulerSize - textSize.Y) * 0.5f
                    );
                    drawList.AddText(textPos, textColor, labelText);
                }
            }
            
            // 绘制底部水平标尺（X轴）
            var bottomRulerRect = new System.Numerics.Vector2(
                viewportStartPos.X,
                viewportStartPos.Y + viewportSize.Y
            );
            var bottomRulerRectMax = new System.Numerics.Vector2(
                viewportStartPos.X + viewportSize.X,
                viewportStartPos.Y + viewportSize.Y + rulerSize
            );
            drawList.AddRectFilled(bottomRulerRect, bottomRulerRectMax, rulerBgColor);
            
            for (float worldX = startX; worldX <= endX; worldX += minorTickSpacing)
            {
                var worldPos = new XnaVector2(worldX, 0);
                var screenPos = WorldToScreen(worldPos);
                var screenX = screenPos.X;
                
                if (screenX < viewportStartPos.X || screenX > viewportStartPos.X + viewportSize.X)
                    continue;
                
                bool isMajor = Math.Abs(worldX % majorTickSpacing) < 0.1f;
                float tickLength = isMajor ? 12f : 6f;
                
                var tickStart = new System.Numerics.Vector2(screenX, viewportStartPos.Y + viewportSize.Y);
                var tickEnd = new System.Numerics.Vector2(screenX, viewportStartPos.Y + viewportSize.Y + tickLength);
                drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
                
                if (isMajor)
                {
                    var labelText = ((int)worldX).ToString();
                    var textSize = ImGui.CalcTextSize(labelText);
                    var textPos = new System.Numerics.Vector2(
                        screenX - textSize.X * 0.5f,
                        viewportStartPos.Y + viewportSize.Y + (rulerSize - textSize.Y) * 0.5f
                    );
                    drawList.AddText(textPos, textColor, labelText);
                }
            }
            
            // 绘制左侧垂直标尺（Y轴）
            var leftRulerRect = new System.Numerics.Vector2(
                viewportStartPos.X - rulerSize,
                viewportStartPos.Y
            );
            var leftRulerRectMax = new System.Numerics.Vector2(
                viewportStartPos.X,
                viewportStartPos.Y + viewportSize.Y
            );
            drawList.AddRectFilled(leftRulerRect, leftRulerRectMax, rulerBgColor);
            
            var startY = (float)(Math.Floor(worldMinY / minorTickSpacing) * minorTickSpacing);
            var endY = (float)(Math.Ceiling(worldMaxY / minorTickSpacing) * minorTickSpacing);
            
            for (float worldY = startY; worldY <= endY; worldY += minorTickSpacing)
            {
                var worldPos = new XnaVector2(0, worldY);
                var screenPos = WorldToScreen(worldPos);
                var screenY = screenPos.Y;
                
                if (screenY < viewportStartPos.Y || screenY > viewportStartPos.Y + viewportSize.Y)
                    continue;
                
                bool isMajor = Math.Abs(worldY % majorTickSpacing) < 0.1f;
                float tickLength = isMajor ? 12f : 6f;
                
                var tickStart = new System.Numerics.Vector2(viewportStartPos.X - tickLength, screenY);
                var tickEnd = new System.Numerics.Vector2(viewportStartPos.X, screenY);
                drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
                
                if (isMajor)
                {
                    var labelText = ((int)worldY).ToString();
                    var textSize = ImGui.CalcTextSize(labelText);
                    // 垂直文字需要旋转，但ImGui不支持，所以使用水平文字
                    var textPos = new System.Numerics.Vector2(
                        viewportStartPos.X - rulerSize + (rulerSize - textSize.X) * 0.5f,
                        screenY - textSize.Y * 0.5f
                    );
                    drawList.AddText(textPos, textColor, labelText);
                }
            }
            
            // 绘制右侧垂直标尺（Y轴）
            var rightRulerRect = new System.Numerics.Vector2(
                viewportStartPos.X + viewportSize.X,
                viewportStartPos.Y
            );
            var rightRulerRectMax = new System.Numerics.Vector2(
                viewportStartPos.X + viewportSize.X + rulerSize,
                viewportStartPos.Y + viewportSize.Y
            );
            drawList.AddRectFilled(rightRulerRect, rightRulerRectMax, rulerBgColor);
            
            for (float worldY = startY; worldY <= endY; worldY += minorTickSpacing)
            {
                var worldPos = new XnaVector2(0, worldY);
                var screenPos = WorldToScreen(worldPos);
                var screenY = screenPos.Y;
                
                if (screenY < viewportStartPos.Y || screenY > viewportStartPos.Y + viewportSize.Y)
                    continue;
                
                bool isMajor = Math.Abs(worldY % majorTickSpacing) < 0.1f;
                float tickLength = isMajor ? 12f : 6f;
                
                var tickStart = new System.Numerics.Vector2(viewportStartPos.X + viewportSize.X, screenY);
                var tickEnd = new System.Numerics.Vector2(viewportStartPos.X + viewportSize.X + tickLength, screenY);
                drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
                
                if (isMajor)
                {
                    var labelText = ((int)worldY).ToString();
                    var textSize = ImGui.CalcTextSize(labelText);
                    var textPos = new System.Numerics.Vector2(
                        viewportStartPos.X + viewportSize.X + (rulerSize - textSize.X) * 0.5f,
                        screenY - textSize.Y * 0.5f
                    );
                    drawList.AddText(textPos, textColor, labelText);
                }
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
            Matrix viewMatrix = Matrix.CreateTranslation(new XnaVector3(-_cameraPosition, 0)) *
                               Matrix.CreateScale(_cameraZoom);
            
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: viewMatrix
            );
            
            // 绘制辅助网格（在场景内容之下）
            DrawGrid(spriteBatch);
            
            // 绘制坐标轴（在网格之上，场景内容之下）
            DrawCoordinateAxes(spriteBatch);
            
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
                        _lastMousePos = new XnaVector2(mousePos.X, mousePos.Y);
                    }
                    else
                    {
                        var currentMousePos = new XnaVector2(mousePos.X, mousePos.Y);
                        var delta = (currentMousePos - _lastMousePos) / _cameraZoom;
                        _cameraPosition -= delta;
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
            
            // 绘制标尺
            DrawRulers();
            
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
