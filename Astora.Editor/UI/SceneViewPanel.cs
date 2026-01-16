using System;
using System.Collections.Generic;
using System.Linq;
using Astora.Core.Scene;
using Astora.Editor.Services;
using Astora.Editor.Tools;
using Astora.Editor.UI.Overlays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Astora.Editor.UI;

/// <summary>
/// 工具模式枚举
/// </summary>
public enum ToolMode
{
    Select,
    Move,
    Rotate
}

/// <summary>
/// 场景视图面板 - 显示场景的编辑器视图，包含网格、坐标轴、Gizmo等辅助内容
/// </summary>
public class SceneViewPanel
{
    private readonly SceneTree _sceneTree;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly Editor _editor;
    private readonly RenderService _renderService;
    
    // 核心组件
    private readonly SceneViewCamera _camera;
    private readonly SceneViewRenderer _renderer;
    private readonly SceneViewInputHandler _inputHandler;
    
    // 工具
    private readonly GizmoRenderer _gizmoRenderer;
    private readonly SelectionTool _selectionTool;
    private readonly MoveTool _moveTool;
    private readonly RotateTool _rotateTool;
    
    // 覆盖层
    private readonly List<ISceneViewOverlay> _overlays;
    
    public SceneViewPanel(SceneTree sceneTree, ImGuiRenderer imGuiRenderer, Editor editor)
    {
        _sceneTree = sceneTree;
        _imGuiRenderer = imGuiRenderer;
        _editor = editor;
        _renderService = new RenderService();
        
        // 初始化相机
        _camera = new SceneViewCamera();
        
        // 初始化渲染器
        _renderer = new SceneViewRenderer(sceneTree, imGuiRenderer);
        
        // 初始化工具
        _gizmoRenderer = new GizmoRenderer();
        _selectionTool = new SelectionTool(sceneTree);
        _moveTool = new MoveTool();
        _rotateTool = new RotateTool();
        
        // 初始化输入处理器
        _inputHandler = new SceneViewInputHandler(
            _camera,
            editor,
            _selectionTool,
            _moveTool,
            _rotateTool
        );
        
        // 初始化覆盖层
        _overlays = new List<ISceneViewOverlay>
        {
            new GridOverlay(_gizmoRenderer),
            new AxisOverlay(_gizmoRenderer),
            new CameraViewportOverlay(_gizmoRenderer, editor),
            new GizmoOverlay(_gizmoRenderer, editor, GetCurrentTool)
        };
    }
    
    /// <summary>
    /// 获取当前工具（用于 GizmoOverlay）
    /// </summary>
    private ITool GetCurrentTool()
    {
        return _inputHandler.CurrentTool switch
        {
            ToolMode.Select => _selectionTool,
            ToolMode.Move => _moveTool,
            ToolMode.Rotate => _rotateTool,
            _ => _selectionTool
        };
    }
    
    /// <summary>
    /// 更新逻辑（编辑器模式下调用）
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // 输入处理在 RenderUI 中完成
    }
    
    /// <summary>
    /// 绘制场景和覆盖层到 RenderTarget
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (spriteBatch == null)
            return;
        
        if (_renderer == null || !_renderer.IsReady)
            return;
        
        if (_camera == null)
            return;
        
        var graphicsDevice = spriteBatch.GraphicsDevice;
        if (graphicsDevice == null)
            return;
        
        // 保存原始 RenderTarget
        var originalRenderTargets = graphicsDevice.GetRenderTargets();
        
        // 渲染场景（这会设置 RenderTarget）
        _renderer.Draw(_camera, spriteBatch);
        
        // 获取视图矩阵
        Matrix viewMatrix = _camera.GetViewMatrix();
        
        // 使用 SpriteBatch 渲染覆盖层（RenderTarget 已经设置好了）
        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: viewMatrix
        );
        
        // 按渲染顺序绘制覆盖层
        var sortedOverlays = _overlays
            .Where(o => o.Enabled)
            .OrderBy(o => o.RenderOrder)
            .ToList();
        
        foreach (var overlay in sortedOverlays)
        {
            overlay.Draw(spriteBatch, _camera, _sceneTree, _renderer.Width, _renderer.Height);
        }
        
        spriteBatch.End();
        
        // 恢复原始 RenderTarget
        if (originalRenderTargets.Length > 0)
        {
            var originalTarget = originalRenderTargets[0].RenderTarget as RenderTarget2D;
            graphicsDevice.SetRenderTarget(originalTarget);
        }
        else
        {
            graphicsDevice.SetRenderTarget(null);
        }
    }
    
    /// <summary>
    /// 渲染 UI 窗口
    /// </summary>
    public void RenderUI()
    {
        ImGui.Begin("Scene View");
        
        // 工具栏
        RenderToolbar();
        
        // 检查窗口是否被悬停
        bool isWindowHovered = ImGui.IsWindowHovered();
        
        // 处理输入
        _inputHandler.HandleInput(isWindowHovered);
        
        // 绘制标尺
        DrawRulers();
        
        // 渲染场景视图
        var viewportSize = ImGui.GetContentRegionAvail();
        if (viewportSize.X > 0 && viewportSize.Y > 0)
        {
            // 更新 RenderTarget 大小（关键：在 Draw 之前更新）
            _renderer.UpdateRenderTarget((int)viewportSize.X, (int)viewportSize.Y);
            
            // 绘制场景和覆盖层
            var spriteBatch = _renderService.GetSpriteBatch();
            if (spriteBatch != null)
            {
                Draw(spriteBatch);
            }
            
            // 显示渲染结果
            ImGui.Image(
                _renderer.TextureId,
                viewportSize,
                Vector2.Zero,
                Vector2.One,
                Vector4.One
            );
        }
        
        ImGui.End();
    }
    
    /// <summary>
    /// 渲染工具栏
    /// </summary>
    private void RenderToolbar()
    {
        if (ImGui.Button("Select"))
        {
            _inputHandler.CurrentTool = ToolMode.Select;
        }
        ImGui.SameLine();
        if (ImGui.Button("Move"))
        {
            _inputHandler.CurrentTool = ToolMode.Move;
        }
        ImGui.SameLine();
        if (ImGui.Button("Rotate"))
        {
            _inputHandler.CurrentTool = ToolMode.Rotate;
        }
        
        // 显示当前工具
        ImGui.SameLine();
        ImGui.Text($"Tool: {_inputHandler.CurrentTool}");
    }
    
    /// <summary>
    /// 绘制标尺（在ImGui中绘制，显示世界坐标像素值）
    /// </summary>
    private void DrawRulers()
    {
        if (!_renderer.IsReady) return;
        
        var drawList = ImGui.GetWindowDrawList();
        // 获取场景视图的起始位置（与Image使用相同的位置）
        var viewportStartPos = ImGui.GetCursorScreenPos();
        var viewportSize = ImGui.GetContentRegionAvail();
        
        // 标尺尺寸
        const float rulerSize = 25f;
        
        // 计算当前视口在世界坐标系中的范围
        var bounds = _camera.GetViewportBounds(_renderer.Width, _renderer.Height);
        
        var worldMinX = bounds.X;
        var worldMaxX = bounds.X + bounds.Width;
        var worldMinY = bounds.Y;
        var worldMaxY = bounds.Y + bounds.Height;
        
        // 根据缩放级别确定刻度间距
        float majorTickSpacing = 100f;
        float minorTickSpacing = 50f;
        
        // 如果缩放太小，增大刻度间距
        if (_camera.Zoom < 0.5f)
        {
            majorTickSpacing = 200f;
            minorTickSpacing = 100f;
        }
        else if (_camera.Zoom < 0.2f)
        {
            majorTickSpacing = 500f;
            minorTickSpacing = 250f;
        }
        
        // 标尺背景颜色
        var rulerBgColor = ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.2f, 0.8f));
        var tickColor = ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        var textColor = ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
        
        // 绘制顶部水平标尺（X轴）
        var topRulerRect = new Vector2(
            viewportStartPos.X,
            viewportStartPos.Y - rulerSize
        );
        var topRulerRectMax = new Vector2(
            viewportStartPos.X + viewportSize.X,
            viewportStartPos.Y
        );
        drawList.AddRectFilled(topRulerRect, topRulerRectMax, rulerBgColor);
        
        // 计算需要显示的刻度范围
        var startX = (float)(Math.Floor(worldMinX / minorTickSpacing) * minorTickSpacing);
        var endX = (float)(Math.Ceiling(worldMaxX / minorTickSpacing) * minorTickSpacing);
        
        for (float worldX = startX; worldX <= endX; worldX += minorTickSpacing)
        {
            // 将世界坐标转换为屏幕坐标
            var worldPos = new Microsoft.Xna.Framework.Vector2(worldX, 0);
            var screenPos = _camera.WorldToScreen(worldPos);
            var screenX = screenPos.X;
            
            if (screenX < viewportStartPos.X || screenX > viewportStartPos.X + viewportSize.X)
                continue;
            
            bool isMajor = Math.Abs(worldX % majorTickSpacing) < 0.1f;
            float tickLength = isMajor ? 12f : 6f;
            
            // 绘制刻度线
            var tickStart = new Vector2(screenX, viewportStartPos.Y - tickLength);
            var tickEnd = new Vector2(screenX, viewportStartPos.Y);
            drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
            
            // 绘制主要刻度的文字标签
            if (isMajor)
            {
                var labelText = ((int)worldX).ToString();
                var textSize = ImGui.CalcTextSize(labelText);
                var textPos = new Vector2(
                    screenX - textSize.X * 0.5f,
                    viewportStartPos.Y - rulerSize + (rulerSize - textSize.Y) * 0.5f
                );
                drawList.AddText(textPos, textColor, labelText);
            }
        }
        
        // 绘制底部水平标尺（X轴）
        var bottomRulerRect = new Vector2(
            viewportStartPos.X,
            viewportStartPos.Y + viewportSize.Y
        );
        var bottomRulerRectMax = new Vector2(
            viewportStartPos.X + viewportSize.X,
            viewportStartPos.Y + viewportSize.Y + rulerSize
        );
        drawList.AddRectFilled(bottomRulerRect, bottomRulerRectMax, rulerBgColor);
        
        for (float worldX = startX; worldX <= endX; worldX += minorTickSpacing)
        {
            var worldPos = new Microsoft.Xna.Framework.Vector2(worldX, 0);
            var screenPos = _camera.WorldToScreen(worldPos);
            var screenX = screenPos.X;
            
            if (screenX < viewportStartPos.X || screenX > viewportStartPos.X + viewportSize.X)
                continue;
            
            bool isMajor = Math.Abs(worldX % majorTickSpacing) < 0.1f;
            float tickLength = isMajor ? 12f : 6f;
            
            var tickStart = new Vector2(screenX, viewportStartPos.Y + viewportSize.Y);
            var tickEnd = new Vector2(screenX, viewportStartPos.Y + viewportSize.Y + tickLength);
            drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
            
            if (isMajor)
            {
                var labelText = ((int)worldX).ToString();
                var textSize = ImGui.CalcTextSize(labelText);
                var textPos = new Vector2(
                    screenX - textSize.X * 0.5f,
                    viewportStartPos.Y + viewportSize.Y + (rulerSize - textSize.Y) * 0.5f
                );
                drawList.AddText(textPos, textColor, labelText);
            }
        }
        
        // 绘制左侧垂直标尺（Y轴）
        var leftRulerRect = new Vector2(
            viewportStartPos.X - rulerSize,
            viewportStartPos.Y
        );
        var leftRulerRectMax = new Vector2(
            viewportStartPos.X,
            viewportStartPos.Y + viewportSize.Y
        );
        drawList.AddRectFilled(leftRulerRect, leftRulerRectMax, rulerBgColor);
        
        var startY = (float)(Math.Floor(worldMinY / minorTickSpacing) * minorTickSpacing);
        var endY = (float)(Math.Ceiling(worldMaxY / minorTickSpacing) * minorTickSpacing);
        
        for (float worldY = startY; worldY <= endY; worldY += minorTickSpacing)
        {
            var worldPos = new Microsoft.Xna.Framework.Vector2(0, worldY);
            var screenPos = _camera.WorldToScreen(worldPos);
            var screenY = screenPos.Y;
            
            if (screenY < viewportStartPos.Y || screenY > viewportStartPos.Y + viewportSize.Y)
                continue;
            
            bool isMajor = Math.Abs(worldY % majorTickSpacing) < 0.1f;
            float tickLength = isMajor ? 12f : 6f;
            
            var tickStart = new Vector2(viewportStartPos.X - tickLength, screenY);
            var tickEnd = new Vector2(viewportStartPos.X, screenY);
            drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
            
            if (isMajor)
            {
                var labelText = ((int)worldY).ToString();
                var textSize = ImGui.CalcTextSize(labelText);
                var textPos = new Vector2(
                    viewportStartPos.X - rulerSize + (rulerSize - textSize.X) * 0.5f,
                    screenY - textSize.Y * 0.5f
                );
                drawList.AddText(textPos, textColor, labelText);
            }
        }
        
        // 绘制右侧垂直标尺（Y轴）
        var rightRulerRect = new Vector2(
            viewportStartPos.X + viewportSize.X,
            viewportStartPos.Y
        );
        var rightRulerRectMax = new Vector2(
            viewportStartPos.X + viewportSize.X + rulerSize,
            viewportStartPos.Y + viewportSize.Y
        );
        drawList.AddRectFilled(rightRulerRect, rightRulerRectMax, rulerBgColor);
        
        for (float worldY = startY; worldY <= endY; worldY += minorTickSpacing)
        {
            var worldPos = new Microsoft.Xna.Framework.Vector2(0, worldY);
            var screenPos = _camera.WorldToScreen(worldPos);
            var screenY = screenPos.Y;
            
            if (screenY < viewportStartPos.Y || screenY > viewportStartPos.Y + viewportSize.Y)
                continue;
            
            bool isMajor = Math.Abs(worldY % majorTickSpacing) < 0.1f;
            float tickLength = isMajor ? 12f : 6f;
            
            var tickStart = new Vector2(viewportStartPos.X + viewportSize.X, screenY);
            var tickEnd = new Vector2(viewportStartPos.X + viewportSize.X + tickLength, screenY);
            drawList.AddLine(tickStart, tickEnd, tickColor, isMajor ? 1.5f : 1.0f);
            
            if (isMajor)
            {
                var labelText = ((int)worldY).ToString();
                var textSize = ImGui.CalcTextSize(labelText);
                var textPos = new Vector2(
                    viewportStartPos.X + viewportSize.X + (rulerSize - textSize.X) * 0.5f,
                    screenY - textSize.Y * 0.5f
                );
                drawList.AddText(textPos, textColor, labelText);
            }
        }
    }
}
