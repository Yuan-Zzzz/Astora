using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// 移动工具 - Godot 风格带轴向箭头 Gizmo
/// </summary>
public class MoveTool : ITool
{
    private bool _isDragging = false;
    private Node2D? _draggedNode;
    private Vector2 _dragStartPos;

    public bool OnMouseDown(Vector2 worldPos, Node2D? selectedNode)
    {
        if (selectedNode != null)
        {
            _isDragging = true;
            _draggedNode = selectedNode;
            _dragStartPos = worldPos;
            return true;
        }
        return false;
    }

    public bool OnMouseDrag(Vector2 worldPos, Node2D? selectedNode)
    {
        if (_isDragging && _draggedNode != null)
        {
            var delta = worldPos - _dragStartPos;
            _draggedNode.Position += delta;
            _dragStartPos = worldPos;
            return true;
        }
        return false;
    }

    public bool OnMouseUp(Vector2 worldPos, Node2D? selectedNode)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _draggedNode = null;
            return true;
        }
        return false;
    }

    public void DrawGizmo(SpriteBatch spriteBatch, GizmoRenderer gizmoRenderer, Node2D node, float cameraZoom)
    {
        // Godot 风格：带箭头的 XY 轴 + 中心正方形 + 选中包围盒
        gizmoRenderer.DrawSelectionBox(spriteBatch, node, cameraZoom);
        gizmoRenderer.DrawMoveGizmo(spriteBatch, node, cameraZoom);
    }
}
