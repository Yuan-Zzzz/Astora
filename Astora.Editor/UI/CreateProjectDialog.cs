using Astora.Editor.Project;
using Astora.Editor.Utils;
using ImGuiNET;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 创建项目对话框
    /// </summary>
    public class CreateProjectDialog
    {
        private readonly Editor _editor;
        private string _projectName = "MyGame";
        private string _projectLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private ProjectTemplateType _selectedTemplate = ProjectTemplateType.Minimal;
        private string _errorMessage = string.Empty;
        private bool _shouldShow = false;

        public CreateProjectDialog(Editor editor)
        {
            _editor = editor;
        }

        public void Show()
        {
            _shouldShow = true;
        }

        public void Render()
        {
            if (_shouldShow)
            {
                ImGui.OpenPopup("Create New Project");
                _shouldShow = false;
            }

            var viewport = ImGui.GetMainViewport();
            var center = viewport.GetCenter();
            
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 300), ImGuiCond.Appearing);

            if (ImGui.BeginPopupModal("Create New Project"))
            {
                ImGui.Text("Create New Project");
                ImGui.Separator();
                ImGui.Spacing();

                // 项目名称
                ImGui.Text("Project Name:");
                ImGui.InputText("##ProjectName", ref _projectName, 256);
                
                ImGui.Spacing();

                // 项目位置
                ImGui.Text("Location:");
                ImGui.InputText("##ProjectLocation", ref _projectLocation, 512);
                ImGui.SameLine();
                if (ImGui.Button("Browse..."))
                {
                    var path = ShowFolderDialog("Select Project Location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        _projectLocation = path;
                    }
                }

                ImGui.Spacing();

                // 项目模板
                ImGui.Text("Template:");
                var templates = new[] { "Minimal", "Empty" };
                var currentTemplate = (int)_selectedTemplate;
                if (ImGui.Combo("##Template", ref currentTemplate, templates, templates.Length))
                {
                    _selectedTemplate = (ProjectTemplateType)currentTemplate;
                }

                ImGui.Spacing();

                // 错误消息
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), _errorMessage);
                    ImGui.Spacing();
                }

                ImGui.Separator();

                // 按钮
                var buttonWidth = (ImGui.GetWindowWidth() - ImGui.GetStyle().ItemSpacing.X) / 2;
                
                if (ImGui.Button("Create", new System.Numerics.Vector2(buttonWidth, 0)))
                {
                    if (ValidateInput())
                    {
                        try
                        {
                            var projectInfo = _editor.ProjectManager.CreateProject(
                                _projectName,
                                _projectLocation,
                                _selectedTemplate
                            );
                            
                            // 加载创建的项目
                            _editor.LoadProject(projectInfo.ProjectPath);
                            
                            // 关闭对话框
                            ImGui.CloseCurrentPopup();
                            _errorMessage = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            _errorMessage = $"Error: {ex.Message}";
                        }
                    }
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel", new System.Numerics.Vector2(buttonWidth, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    _errorMessage = string.Empty;
                }

                ImGui.EndPopup();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_projectName))
            {
                _errorMessage = "Project name cannot be empty";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_projectLocation))
            {
                _errorMessage = "Project location cannot be empty";
                return false;
            }

            if (!Directory.Exists(_projectLocation))
            {
                _errorMessage = "Project location does not exist";
                return false;
            }

            var projectPath = Path.Combine(_projectLocation, _projectName);
            if (Directory.Exists(projectPath))
            {
                _errorMessage = "Project directory already exists";
                return false;
            }

            _errorMessage = string.Empty;
            return true;
        }

        private string ShowFolderDialog(string title)
        {
            return FileDialog.ShowFolderDialog(title, _projectLocation);
        }
    }
}

