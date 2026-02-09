using Astora.Editor.Core;
using Astora.Editor.Core.Modules;
using Astora.Editor.UI;

namespace Astora.Editor.Modules;

/// <summary>
/// 核心模块：菜单栏、通知等全局 UI。
/// </summary>
public sealed class CoreModule : IEditorModule
{
    public string Id => "core";

    private readonly IEditorContext _ctx;
    public CreateProjectDialog CreateProjectDialog { get; }
    public MenuBar MenuBar { get; }
    private readonly NotificationPanel _notificationPanel;

    public CoreModule(IEditorContext ctx)
    {
        _ctx = ctx;
        CreateProjectDialog = new CreateProjectDialog(_ctx);
        MenuBar = new MenuBar(_ctx, CreateProjectDialog.Show);
        _notificationPanel = new NotificationPanel(_ctx.EditorService.State.NotificationManager);
    }

    public void Register(EditorModuleRegistry registry)
    {
        registry.AddImGuiRenderer(MenuBar.Render);
        registry.AddImGuiRenderer(CreateProjectDialog.Render);
        registry.AddImGuiRenderer(_notificationPanel.Render);
    }
}

