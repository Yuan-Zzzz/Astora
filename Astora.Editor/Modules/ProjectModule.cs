using Astora.Editor.Core;
using Astora.Editor.Core.Modules;
using Astora.Editor.UI;
using ImGuiNET;

namespace Astora.Editor.Modules;

/// <summary>
/// 项目模块：启动器、项目/资源面板。
/// </summary>
public sealed class ProjectModule : IEditorModule
{
    public string Id => "project";

    private readonly IEditorContext _ctx;
    private readonly ProjectLauncherPanel _launcher;
    private readonly ProjectPanel _projectPanel;
    private readonly AssetPanel _assetPanel;

    // 复用 CoreModule 的对话框/菜单触发方式：这里直接弹出“Open Project”由菜单内部处理。
    public ProjectModule(IEditorContext ctx, MenuBar menuBar, CreateProjectDialog createProjectDialog)
    {
        _ctx = ctx;
        _launcher = new ProjectLauncherPanel(_ctx, menuBar.ShowOpenProjectDialog, createProjectDialog.Show);
        _projectPanel = new ProjectPanel(_ctx.ProjectService.ProjectManager, _ctx.ProjectService.SceneManager, _ctx);
        _assetPanel = new AssetPanel(_ctx.ProjectService.ProjectManager, _ctx);
    }

    public void Register(EditorModuleRegistry registry)
    {
        registry.AddImGuiRenderer(Render);
    }

    private void Render()
    {
        if (!_ctx.EditorService.State.IsProjectLoaded)
        {
            _launcher.Render();
            return;
        }

        // DockSpace 统一在这里建立（项目已加载状态）
        var viewport = ImGui.GetMainViewport();
        ImGui.DockSpaceOverViewport(viewport.ID);

        _projectPanel.Render();
        _assetPanel.Render();
    }
}

