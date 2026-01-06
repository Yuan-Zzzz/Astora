using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// 旋转工具，用于在场景中旋转节点
/// </summary>
public class RotateTool : ITool
{
    private bool _isDragging = false;
    private Node2D? _draggedNode;
    private float _rotateStartAngle;
    
    public bool OnMouseDown(Vector2 worldPos, Node2D? selectedNode)
    {
        if (selectedNode != null)
        {
            _isDragging = true;
            _draggedNode = selectedNode;
            var nodeWorldPos = selectedNode.GlobalPosition;
            var toMouse = worldPos - nodeWorldPos;
            _rotateStartAngle = (float)Math.Atan2(toMouse.Y, toMouse.X);
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
            
            // 如果父节点是Node2D，需要考虑父节点的旋转
            if (_draggedNode.Parent is Node2D parent2d)
            {
                angleDelta -= parent2d.Rotation;
            }
            
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
        gizmoRenderer.DrawRotateGizmo(spriteBatch, node, cameraZoom);
    }
}

