using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// 旋转工具 - Godot 风格蓝色圆环 Gizmo
/// </summary>
public class RotateTool : ITool
{
    private bool _isDragging = false;
    private Node2D? _draggedNode;
    private float _rotateStartAngle;
    private float _dragStartAngle;

    public bool IsDragging => _isDragging;
    public float DragStartAngle => _dragStartAngle;
    public float CurrentAngle => _rotateStartAngle;

    public bool OnMouseDown(Vector2 worldPos, Node2D? selectedNode)
    {
        if (selectedNode != null)
        {
            _isDragging = true;
            _draggedNode = selectedNode;
            var nodeWorldPos = selectedNode.GlobalPosition;
            var toMouse = worldPos - nodeWorldPos;
            _rotateStartAngle = (float)Math.Atan2(toMouse.Y, toMouse.X);
            _dragStartAngle = _rotateStartAngle;
            return true;
        }
        return false;
    }

    public bool OnMouseDrag(Vector2 worldPos, Node2D? selectedNode)
    {
        if (_isDragging && _draggedNode != null)
        {
            var nodeWorldPos = _draggedNode.GlobalPosition;
            var toMouse = worldPos - nodeWorldPos;
            var currentAngle = (float)Math.Atan2(toMouse.Y, toMouse.X);
            var angleDelta = currentAngle - _rotateStartAngle;

            _draggedNode.Rotation += angleDelta;
            _rotateStartAngle = currentAngle;
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
        // Godot 风格：蓝色旋转圆环 + 选中包围盒
        gizmoRenderer.DrawSelectionBox(spriteBatch, node, cameraZoom);
        gizmoRenderer.DrawRotateGizmo(spriteBatch, node, cameraZoom,
            hover: false,
            isDragging: _isDragging,
            dragStartAngle: _dragStartAngle,
            currentAngle: _rotateStartAngle);
    }
}
