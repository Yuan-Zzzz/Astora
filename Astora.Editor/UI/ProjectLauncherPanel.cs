using Astora.Editor.Project;
using Astora.Editor.Core;
using ImGuiNET;
using System.Numerics;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 项目启动面板 - Godot 风格：卡片式最近项目 + 加载进度指示
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
            var viewport = ImGui.GetMainViewport();
            var center = viewport.GetCenter();

            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(700, 520), ImGuiCond.Always);

            ImGui.Begin("##Launcher",
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);

            // === 标题区域 ===
            RenderHeader();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // === 如果正在加载项目，显示进度 ===
            if (_ctx.EditorService.State.IsLoadingProject)
            {
                RenderLoadingProgress();
                ImGui.End();
                return;
            }

            // === 加载错误提示 ===
            if (_ctx.EditorService.State.LoadState == ProjectLoadState.Error)
            {
                ImGui.TextColored(new Vector4(1f, 0.35f, 0.35f, 1f),
                    $"Load failed: {_ctx.EditorService.State.LoadError ?? "Unknown error"}");
                ImGui.Spacing();
            }

            // === 最近项目列表 ===
            RenderRecentProjects();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // === 操作按钮 ===
            RenderActionButtons();

            // === 底部版本信息 ===
            RenderFooter();

            ImGui.End();
        }

        private void RenderHeader()
        {
            ImGui.Dummy(new Vector2(0, 4));

            // 居中大标题 — 使用 Dummy + SameLine 居中，避免 SetCursorPos 扩展边界
            var title = "Astora Engine";
            var titleSize = ImGui.CalcTextSize(title);
            float indent = (ImGui.GetContentRegionAvail().X - titleSize.X) * 0.5f;
            if (indent > 0) ImGui.Indent(indent);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiStyleManager.GetAccentColor());
            ImGui.Text(title);
            ImGui.PopStyleColor();
            if (indent > 0) ImGui.Unindent(indent);

            // 副标题
            var subtitle = "Project Manager";
            var subSize = ImGui.CalcTextSize(subtitle);
            float subIndent = (ImGui.GetContentRegionAvail().X - subSize.X) * 0.5f;
            if (subIndent > 0) ImGui.Indent(subIndent);
            ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), subtitle);
            if (subIndent > 0) ImGui.Unindent(subIndent);
        }

        private void RenderLoadingProgress()
        {
            var state = _ctx.EditorService.State;

            ImGui.Spacing();
            ImGui.Spacing();

            // 阶段文字（居中）
            var msg = state.LoadMessage;
            var msgSize = ImGui.CalcTextSize(msg);
            float msgIndent = (ImGui.GetContentRegionAvail().X - msgSize.X) * 0.5f;
            if (msgIndent > 0) ImGui.Indent(msgIndent);
            ImGui.Text(msg);
            if (msgIndent > 0) ImGui.Unindent(msgIndent);

            ImGui.Spacing();

            // 进度条（居中 + 固定宽度）
            float barWidth = Math.Min(400, ImGui.GetContentRegionAvail().X - 20);
            float barIndent = (ImGui.GetContentRegionAvail().X - barWidth) * 0.5f;
            if (barIndent > 0) ImGui.Indent(barIndent);
            ImGui.ProgressBar(state.LoadProgress, new Vector2(barWidth, 6), "");
            if (barIndent > 0) ImGui.Unindent(barIndent);

            // 百分比文字
            var pctText = $"{(int)(state.LoadProgress * 100)}%";
            var pctSize = ImGui.CalcTextSize(pctText);
            float pctIndent = (ImGui.GetContentRegionAvail().X - pctSize.X) * 0.5f;
            if (pctIndent > 0) ImGui.Indent(pctIndent);
            ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), pctText);
            if (pctIndent > 0) ImGui.Unindent(pctIndent);
        }

        private void RenderRecentProjects()
        {
            ImGui.Text("Recent Projects");
            ImGui.Spacing();

            var recentProjects = _ctx.ProjectService.ProjectManager.GetRecentProjects();
            if (recentProjects.Count == 0)
            {
                ImGui.Spacing();
                var emptyMsg = "No recent projects";
                var emptySize = ImGui.CalcTextSize(emptyMsg);
                float emptyIndent = (ImGui.GetContentRegionAvail().X - emptySize.X) * 0.5f;
                if (emptyIndent > 0) ImGui.Indent(emptyIndent);
                ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), emptyMsg);
                if (emptyIndent > 0) ImGui.Unindent(emptyIndent);
                ImGui.Spacing();
                return;
            }

            // 滚动区域
            ImGui.BeginChild("RecentProjects", new Vector2(0, 250), ImGuiChildFlags.Borders);

            foreach (var project in recentProjects)
            {
                RenderProjectCard(project);
            }

            ImGui.EndChild();
        }

        /// <summary>
        /// 卡片式项目条目 — 使用 DrawList overlay 代替 SetCursorScreenPos
        /// </summary>
        private void RenderProjectCard(RecentProjectInfo project)
        {
            var projectName = Path.GetFileNameWithoutExtension(project.Path);
            var projectDir = Path.GetDirectoryName(project.Path) ?? "";
            var lastOpened = project.LastOpened.ToString("yyyy-MM-dd HH:mm");
            bool fileExists = File.Exists(project.Path);

            ImGui.PushID(project.Path);

            // 记录卡片起始位置
            var cursorScreenPos = ImGui.GetCursorScreenPos();
            float cardHeight = 52;

            // 不可见的 Selectable 占位整个卡片区域
            if (ImGui.Selectable($"##card", false,
                    ImGuiSelectableFlags.AllowDoubleClick, new Vector2(ImGui.GetContentRegionAvail().X, cardHeight)))
            {
                if (fileExists)
                    _ctx.Actions.LoadProject(project.Path);
                else
                    ProjectSettings.RemoveRecentProject(project.Path);
            }

            // 使用 DrawList 在 Selectable 区域上方绘制文字（不移动 ImGui cursor）
            var drawList = ImGui.GetWindowDrawList();

            // 项目名称
            var nameColor = fileExists
                ? ImGui.GetColorU32(ImGuiStyleManager.GetTextColor())
                : ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 1f));
            drawList.AddText(
                new Vector2(cursorScreenPos.X + 12, cursorScreenPos.Y + 6),
                nameColor, projectName);

            // 路径 + 时间
            var detailText = $"{projectDir}  |  {lastOpened}";
            if (!fileExists) detailText += "  (missing)";

            var detailColor = fileExists
                ? ImGui.GetColorU32(ImGuiStyleManager.GetTextDisabledColor())
                : ImGui.GetColorU32(new Vector4(1f, 0.35f, 0.35f, 0.8f));
            drawList.AddText(
                new Vector2(cursorScreenPos.X + 12, cursorScreenPos.Y + 28),
                detailColor, detailText);

            ImGui.PopID();
        }

        private void RenderActionButtons()
        {
            float buttonHeight = 32;
            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float totalWidth = ImGui.GetContentRegionAvail().X;
            float buttonWidth = (totalWidth - spacing) * 0.5f;

            if (ImGui.Button("Open Project", new Vector2(buttonWidth, buttonHeight)))
                _showOpenProjectDialog();

            ImGui.SameLine();

            if (ImGui.Button("New Project", new Vector2(buttonWidth, buttonHeight)))
                _showCreateProjectDialog();
        }

        private void RenderFooter()
        {
            // 用 Dummy 填充剩余空间，使版本信息自然出现在底部
            var avail = ImGui.GetContentRegionAvail();
            float footerHeight = ImGui.GetTextLineHeightWithSpacing() + 4;
            float spacerHeight = avail.Y - footerHeight;
            if (spacerHeight > 0)
                ImGui.Dummy(new Vector2(0, spacerHeight));

            var version = "Astora Engine v0.1.0";
            var versionSize = ImGui.CalcTextSize(version);
            float vIndent = (ImGui.GetContentRegionAvail().X - versionSize.X) * 0.5f;
            if (vIndent > 0) ImGui.Indent(vIndent);
            ImGui.TextColored(new Vector4(0.4f, 0.4f, 0.4f, 1f), version);
            if (vIndent > 0) ImGui.Unindent(vIndent);
        }
    }
}
