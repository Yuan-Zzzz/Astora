using Astora.Core.Nodes;
using Astora.Editor.Core;
using Astora.Editor.Core.Modules;
using Astora.Editor.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.Modules;

/// <summary>
/// 场景模块：层级、检视器、Scene/Game 视图。
/// </summary>
public sealed class SceneModule : IEditorModule
{
    public string Id => "scene";

    private readonly IEditorContext _ctx;
    private readonly ImGuiRenderer _imGuiRenderer;

    private readonly HierarchyPanel _hierarchy;
    private readonly InspectorPanel _inspector;
    private readonly SceneViewPanel _sceneView;
    private readonly GameViewPanel _gameView;

    public SceneModule(IEditorContext ctx, ImGuiRenderer imGuiRenderer)
    {
        _ctx = ctx;
        _imGuiRenderer = imGuiRenderer;

        _hierarchy = new HierarchyPanel(_ctx.EditorService.SceneTree, _ctx.ProjectService.NodeTypeRegistry);
        _inspector = new InspectorPanel(_ctx.ProjectService.ProjectManager, _imGuiRenderer);
        _sceneView = new SceneViewPanel(_ctx.EditorService.SceneTree, _imGuiRenderer, _ctx);
        _gameView = new GameViewPanel(_ctx.EditorService.SceneTree, _imGuiRenderer, _ctx.ProjectService.ProjectManager, _ctx);
    }

    public void Register(EditorModuleRegistry registry)
    {
        registry.AddViewportDrawer(DrawViewport);
        registry.AddImGuiRenderer(RenderImGui);
    }

    private void DrawViewport(SpriteBatch spriteBatch)
    {
        // Scene 和 Game 视图始终渲染
        _sceneView.Draw(spriteBatch);
        _gameView.Draw(spriteBatch);
    }

    private void RenderImGui()
    {
        if (!_ctx.EditorService.State.IsProjectLoaded)
            return;

        Node? selected = _ctx.Actions.GetSelectedNode();
        _hierarchy.Render(ref selected);
        _ctx.Actions.SetSelectedNode(selected);

        _inspector.Render(selected);

        // Scene 和 Game 视图始终显示（参考 Unity）
        _sceneView.RenderUI();
        _gameView.RenderUI();
    }
}

