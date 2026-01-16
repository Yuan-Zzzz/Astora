using Astora.Core.Nodes;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Astora.Editor.Core;

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
/// 工具管理器 - 统一管理编辑器工具
/// </summary>
public class ToolManager
{
    private readonly Dictionary<ToolMode, ITool> _tools = new Dictionary<ToolMode, ITool>();
    private ToolMode _currentToolMode = ToolMode.Select;
    
    /// <summary>
    /// 当前工具模式
    /// </summary>
    public ToolMode CurrentToolMode
    {
        get => _currentToolMode;
        set
        {
            if (_tools.ContainsKey(value))
            {
                _currentToolMode = value;
            }
        }
    }
    
    /// <summary>
    /// 当前工具
    /// </summary>
    public ITool CurrentTool => _tools[_currentToolMode];
    
    /// <summary>
    /// 注册工具
    /// </summary>
    public void RegisterTool(ToolMode mode, ITool tool)
    {
        _tools[mode] = tool;
    }
    
    /// <summary>
    /// 获取工具
    /// </summary>
    public ITool? GetTool(ToolMode mode)
    {
        return _tools.TryGetValue(mode, out var tool) ? tool : null;
    }
    
    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    public bool OnMouseDown(Vector2 worldPos, Node2D? selectedNode)
    {
        return CurrentTool.OnMouseDown(worldPos, selectedNode);
    }
    
    /// <summary>
    /// 处理鼠标拖拽事件
    /// </summary>
    public bool OnMouseDrag(Vector2 worldPos, Node2D? selectedNode)
    {
        return CurrentTool.OnMouseDrag(worldPos, selectedNode);
    }
    
    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    public bool OnMouseUp(Vector2 worldPos, Node2D? selectedNode)
    {
        return CurrentTool.OnMouseUp(worldPos, selectedNode);
    }
    
    /// <summary>
    /// 绘制当前工具的Gizmo
    /// </summary>
    public void DrawGizmo(SpriteBatch spriteBatch, GizmoRenderer gizmoRenderer, Node2D node, float cameraZoom)
    {
        CurrentTool.DrawGizmo(spriteBatch, gizmoRenderer, node, cameraZoom);
    }
}
