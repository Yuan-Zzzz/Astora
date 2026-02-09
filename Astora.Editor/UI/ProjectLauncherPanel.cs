using Astora.Editor.Project;
using Astora.Editor.Core;
using ImGuiNET;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 项目启动面板 - 显示项目选择/创建界面
    /// </summary>
    public class ProjectLauncherPanel
    {
        private readonly IEditorContext _ctx;
        private readonly Action _showOpenProjectDialog;
        private readonly Action _showCreateProjectDialog;

        public ProjectLauncherPanel(IEditorContext ctx, Action showOpenProjectDialog, Action showCreateProjectDialog)
        {
            _ctx = ctx;
            _showOpenProjectDialog = showOpenProjectDialog;
            _showCreateProjectDialog = showCreateProjectDialog;
        }

        public void Render()
        {
            // 全屏居中显示
            var viewport = ImGui.GetMainViewport();
            var center = viewport.GetCenter();
            
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 500), ImGuiCond.Always);
            
            ImGui.Begin("Welcome to Astora Editor", 
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);

            // 标题
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize("Welcome to Astora Editor").X) * 0.5f);
            ImGui.Text("Welcome to Astora Editor");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // 最近项目
            ImGui.Text("Recent Projects");
            ImGui.Separator();
            
            var recentProjects = _ctx.ProjectService.ProjectManager.GetRecentProjects();
            if (recentProjects.Count == 0)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1.0f), "No recent projects");
            }
            else
            {
                ImGui.BeginChild("RecentProjects", new System.Numerics.Vector2(0, 200));
                
                foreach (var project in recentProjects)
                {
                    var projectName = Path.GetFileNameWithoutExtension(project.Path);
                    var projectPath = project.Path;
                    var lastOpened = project.LastOpened.ToString("yyyy-MM-dd HH:mm");
                    
                    if (ImGui.Selectable($"{projectName}##{projectPath}"))
                    {
                        if (File.Exists(projectPath))
                        {
                            _ctx.Actions.LoadProject(projectPath);
                        }
                        else
                        {
                            // 项目文件不存在，从最近列表中移除
                            ProjectSettings.RemoveRecentProject(projectPath);
                        }
                    }
                    
                    // 显示项目路径和最后打开时间
                    ImGui.SameLine();
                    ImGui.TextColored(new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1.0f), 
                        $"({Path.GetDirectoryName(projectPath)}) - {lastOpened}");
                }
                
                ImGui.EndChild();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // 按钮
            var buttonWidth = (ImGui.GetWindowWidth() - ImGui.GetStyle().ItemSpacing.X) / 2;
            
            if (ImGui.Button("Open Project", new System.Numerics.Vector2(buttonWidth, 0)))
            {
                _showOpenProjectDialog();
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Create New Project", new System.Numerics.Vector2(buttonWidth, 0)))
            {
                _showCreateProjectDialog();
            }

            ImGui.End();
        }
    }
}

