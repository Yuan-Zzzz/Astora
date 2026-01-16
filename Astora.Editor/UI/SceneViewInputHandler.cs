using Astora.Core.Nodes;
using Astora.Editor.Tools;
using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.UI;

/// <summary>
/// 场景视图输入处理器，处理鼠标和键盘输入
/// </summary>
public class SceneViewInputHandler
{
    private readonly SceneViewCamera _camera;
    private readonly Editor _editor;
    private readonly SelectionTool _selectionTool;
    private readonly MoveTool _moveTool;
    private readonly RotateTool _rotateTool;
    private ToolMode _currentTool = ToolMode.Select;
    
    // 输入状态
    private bool _isPanning = false;
    private XnaVector2 _lastMousePos;
    
    public SceneViewInputHandler(
        SceneViewCamera camera,
        Editor editor,
        SelectionTool selectionTool,
        MoveTool moveTool,
        RotateTool rotateTool)
    {
        _camera = camera;
        _editor = editor;
        _selectionTool = selectionTool;
        _moveTool = moveTool;
        _rotateTool = rotateTool;
    }
    
    /// <summary>
    /// 当前工具模式
    /// </summary>
    public ToolMode CurrentTool
    {
        get => _currentTool;
        set => _currentTool = value;
    }
    
    /// <summary>
    /// 处理输入事件（在 RenderUI 中调用）
    /// </summary>
    public void HandleInput(bool isWindowHovered)
    {
        if (!isWindowHovered)
        {
            _isPanning = false;
            return;
        }
        
        // 获取鼠标位置
        var mousePos = ImGui.GetMousePos();
        
        // 处理相机控制（中键拖拽平移，滚轮缩放）
        HandleCameraInput(mousePos);
        
        // 处理节点选择和操作
        if (!_isPanning)
        {
            HandleNodeInteraction(mousePos);
        }
    }
    
    /// <summary>
    /// 处理相机输入
    /// </summary>
    private void HandleCameraInput(Vector2 mousePos)
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
                var delta = (currentMousePos - _lastMousePos) / _camera.Zoom;
                _camera.Pan(delta);
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
                _camera.ZoomIn(zoomFactor);
            }
            else
            {
                _camera.ZoomOut(zoomFactor);
            }
        }
    }
    
    /// <summary>
    /// 处理节点交互
    /// </summary>
    private void HandleNodeInteraction(Vector2 mousePos)
    {
        var worldPos = _camera.ScreenToWorld(mousePos);
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
}
