using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Editor.Core.Actions;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.UI.Overlays;

/// <summary>
/// Gizmo 覆盖层 - 绘制选中节点的 Gizmo 和包围盒
/// </summary>
public class GizmoOverlay : ISceneViewOverlay
{
    private readonly GizmoRenderer _gizmoRenderer;
    private readonly IEditorActions _actions;
    private readonly Func<ITool> _getCurrentTool;

    public bool Enabled { get; set; } = true;
    public int RenderOrder => 4; // 最后渲染，在所有内容之上

    public GizmoOverlay(GizmoRenderer gizmoRenderer, IEditorActions actions, Func<ITool> getCurrentTool)
    {
        _gizmoRenderer = gizmoRenderer;
        _actions = actions;
        _getCurrentTool = getCurrentTool;
    }

    public void Draw(SpriteBatch spriteBatch, SceneViewCamera camera, SceneTree sceneTree, int viewportWidth, int viewportHeight)
    {
        if (!Enabled) return;

        var selectedNode = _actions.GetSelectedNode();
        if (selectedNode is not Node2D node2d) return;

        var currentTool = _getCurrentTool();
        currentTool.DrawGizmo(spriteBatch, _gizmoRenderer, node2d, camera.Zoom);
    }
}
