using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.Core.Modules;

/// <summary>
/// 模块宿主：负责创建模块、收集钩子并在每帧调用。
/// </summary>
public sealed class EditorModuleHost
{
    private readonly EditorModuleRegistry _registry = new();

    public IReadOnlyList<Action> ImGuiRenderers => _registry.ImGuiRenderers;
    public IReadOnlyList<Action<SpriteBatch>> ViewportDrawers => _registry.ViewportDrawers;
    public IReadOnlyList<Action> ShortcutHandlers => _registry.ShortcutHandlers;

    public void Load(params IEditorModule[] modules)
    {
        foreach (var m in modules)
            m.Register(_registry);
    }
}

