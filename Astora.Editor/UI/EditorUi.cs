using Astora.Editor.Core;
using Astora.Editor.Core.Modules;
using Astora.Editor.Modules;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.UI;

/// <summary>
/// Editor UI 组合根：通过模块加载扩展点，不再硬编码组合。
/// </summary>
public sealed class EditorUi
{
    private readonly IEditorContext _ctx;
    private readonly EditorModuleHost _host;

    public EditorUi(IEditorContext ctx, ImGuiRenderer imGuiRenderer)
    {
        _ctx = ctx;
        _host = new EditorModuleHost();

        // 模块列表（后续可替换为反射/配置加载）
        var core = new CoreModule(_ctx);
        _host.Load(
            core,
            new ProjectModule(_ctx, core.MenuBar, core.CreateProjectDialog),
            new SceneModule(_ctx, imGuiRenderer)
        );
    }

    /// <summary>
    /// 在 ImGui 之外绘制视口（用于 SceneView / GameView 的 RenderTarget 更新）。
    /// </summary>
    public void DrawViewport(SpriteBatch spriteBatch)
    {
        foreach (var draw in _host.ViewportDrawers)
            draw(spriteBatch);
    }

    public void Render()
    {
        foreach (var handler in _host.ShortcutHandlers)
            handler();

        foreach (var render in _host.ImGuiRenderers)
            render();
    }
}

