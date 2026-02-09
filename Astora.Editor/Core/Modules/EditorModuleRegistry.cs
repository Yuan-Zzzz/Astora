using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.Core.Modules;

/// <summary>
/// 模块注册表：模块通过它把“渲染钩子”注册进 EditorUi。
/// 这是最轻量的插件机制，后续可以继续细分为 Panel/Menu/Shortcut/Tool registry。
/// </summary>
public sealed class EditorModuleRegistry
{
    private readonly List<Action> _imGuiRenderers = new();
    private readonly List<Action<SpriteBatch>> _viewportDrawers = new();
    private readonly List<Action> _shortcutHandlers = new();

    public IReadOnlyList<Action> ImGuiRenderers => _imGuiRenderers;
    public IReadOnlyList<Action<SpriteBatch>> ViewportDrawers => _viewportDrawers;
    public IReadOnlyList<Action> ShortcutHandlers => _shortcutHandlers;

    public void AddImGuiRenderer(Action render) => _imGuiRenderers.Add(render);
    public void AddViewportDrawer(Action<SpriteBatch> draw) => _viewportDrawers.Add(draw);
    public void AddShortcutHandler(Action handle) => _shortcutHandlers.Add(handle);
}

