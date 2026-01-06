using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// 选择工具，用于在场景中选择节点
/// </summary>
public class SelectionTool : ITool
{
    private readonly SceneTree _sceneTree;
    
    public SelectionTool(SceneTree sceneTree)
    {
        _sceneTree = sceneTree;
    }
    
    public bool OnMouseDown(Vector2 worldPos, Node2D? selectedNode)
    {
        // 选择工具不处理鼠标按下，选择逻辑由 SceneViewPanel 处理
        return false;
    }
    
    public bool OnMouseDrag(Vector2 worldPos, Node2D? selectedNode)
    {
        // 选择工具不处理拖拽
        return false;
    }
    
    public bool OnMouseUp(Vector2 worldPos, Node2D? selectedNode)
    {
        // 选择工具不处理鼠标释放
        return false;
    }
    
    public void DrawGizmo(SpriteBatch spriteBatch, GizmoRenderer gizmoRenderer, Node2D node, float cameraZoom)
    {
        // 选择工具不绘制 Gizmo
    }
    
    /// <summary>
    /// 查找被点击的节点
    /// </summary>
    public Node2D? FindNodeAtPosition(Vector2 worldPos)
    {
        return FindNodeAtPosition(worldPos, _sceneTree.Root);
    }
    
    /// <summary>
    /// 递归查找被点击的节点
    /// </summary>
    private Node2D? FindNodeAtPosition(Vector2 worldPos, Node? node)
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
                var size = new Vector2(texture.Width * sprite.Scale.X, texture.Height * sprite.Scale.Y);
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
}

