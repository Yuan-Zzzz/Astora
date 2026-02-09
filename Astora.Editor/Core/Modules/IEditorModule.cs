namespace Astora.Editor.Core.Modules;

/// <summary>
/// Editor 模块（插件）入口：在这里注册窗口、菜单、快捷键、工具等扩展点。
/// </summary>
public interface IEditorModule
{
    string Id { get; }
    void Register(EditorModuleRegistry registry);
}

