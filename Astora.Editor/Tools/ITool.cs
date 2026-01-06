using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// 工具接口，定义编辑器工具的基本行为
/// </summary>
public interface ITool
{
    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    /// <param name="worldPos">世界坐标位置</param>
    /// <param name="selectedNode">当前选中的节点</param>
    /// <returns>是否处理了该事件</returns>
    bool OnMouseDown(Vector2 worldPos, Node2D? selectedNode);
    
    /// <summary>
    /// 处理鼠标拖拽事件
    /// </summary>
    /// <param name="worldPos">当前世界坐标位置</param>
    /// <param name="selectedNode">当前选中的节点</param>
    /// <returns>是否处理了该事件</returns>
    bool OnMouseDrag(Vector2 worldPos, Node2D? selectedNode);
    
    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    /// <param name="worldPos">世界坐标位置</param>
    /// <param name="selectedNode">当前选中的节点</param>
    /// <returns>是否处理了该事件</returns>
    bool OnMouseUp(Vector2 worldPos, Node2D? selectedNode);
    
    /// <summary>
    /// 绘制 Gizmo
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch 实例</param>
    /// <param name="gizmoRenderer">GizmoRenderer 实例</param>
    /// <param name="node">要绘制 Gizmo 的节点</param>
    /// <param name="cameraZoom">相机缩放值</param>
    void DrawGizmo(SpriteBatch spriteBatch, GizmoRenderer gizmoRenderer, Node2D node, float cameraZoom);
}

