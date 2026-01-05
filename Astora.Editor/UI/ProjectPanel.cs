using Astora.Editor.Project;
using ImGuiNET;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 项目面板 - 显示项目结构和场景列表
    /// </summary>
    public class ProjectPanel
    {
        private readonly ProjectManager _projectManager;
        private readonly SceneManager _sceneManager;
        private readonly Editor _editor;
        private string _newSceneName = "NewScene";

        public ProjectPanel(ProjectManager projectManager, SceneManager sceneManager, Editor editor)
        {
            _projectManager = projectManager;
            _sceneManager = sceneManager;
            _editor = editor;
        }

        public void Render()
        {
            ImGui.Begin("Project");

            if (!_projectManager.HasProject)
            {
                ImGui.Text("No project opened");
                ImGui.Text("Please open a project via File > Open Project");
                ImGui.End();
                return;
            }

            var project = _projectManager.CurrentProject;
            if (project == null)
            {
                ImGui.End();
                return;
            }

            // Display project info
            ImGui.Text($"Project: {Path.GetFileName(project.ProjectPath)}");
            ImGui.Separator();

            // Scene list
            ImGui.Text("Scenes");
            ImGui.Separator();

            // Refresh scene list
            if (ImGui.Button("Refresh"))
            {
                _sceneManager.ScanScenes();
            }

            ImGui.SameLine();
            if (ImGui.Button("New Scene"))
            {
                ImGui.OpenPopup("CreateScenePopup");
            }

            // Create scene popup
            if (ImGui.BeginPopupModal("CreateScenePopup"))
            {
                ImGui.Text("Create New Scene");
                ImGui.InputText("Scene Name", ref _newSceneName, 256);
                
                if (ImGui.Button("Create"))
                {
                    if (!string.IsNullOrWhiteSpace(_newSceneName))
                    {
                        _editor.CreateNewScene(_newSceneName);
                        _newSceneName = "NewScene";
                        ImGui.CloseCurrentPopup();
                    }
                }
                
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }

            ImGui.Separator();

            // Display scene list
            var scenes = project.Scenes;
            if (scenes.Count == 0)
            {
                ImGui.Text("No scene files");
            }
            else
            {
                foreach (var scenePath in scenes)
                {
                    var sceneName = _sceneManager.GetSceneName(scenePath);
                    var isCurrentScene = _editor.CurrentScenePath == scenePath;
                    
                    if (isCurrentScene)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 0, 1)); // Yellow highlight
                    }

                    // Display scene name, double-click to open
                    if (ImGui.Selectable(sceneName, isCurrentScene))
                    {
                        _editor.LoadScene(scenePath);
                    }

                    if (isCurrentScene)
                    {
                        ImGui.PopStyleColor();
                    }

                    // Right-click context menu
                    if (ImGui.BeginPopupContextItem($"##{sceneName}"))
                    {
                        if (ImGui.MenuItem("Open"))
                        {
                            _editor.LoadScene(scenePath);
                        }
                        
                        if (ImGui.MenuItem("Rename"))
                        {
                            ImGui.OpenPopup($"RenameScene_{sceneName}");
                        }
                        
                        if (ImGui.MenuItem("Delete"))
                        {
                            ImGui.OpenPopup($"DeleteScene_{sceneName}");
                        }
                        
                        ImGui.EndPopup();
                    }

                    // Rename popup
                    if (ImGui.BeginPopupModal($"RenameScene_{sceneName}"))
                    {
                        var newName = sceneName;
                        ImGui.Text($"Rename Scene: {sceneName}");
                        ImGui.InputText("New Name", ref newName, 256);
                        
                        if (ImGui.Button("OK"))
                        {
                            if (!string.IsNullOrWhiteSpace(newName))
                            {
                                _sceneManager.RenameScene(scenePath, newName);
                                ImGui.CloseCurrentPopup();
                            }
                        }
                        
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        
                        ImGui.EndPopup();
                    }

                    // Delete confirmation popup
                    if (ImGui.BeginPopupModal($"DeleteScene_{sceneName}"))
                    {
                        ImGui.Text($"Are you sure you want to delete scene '{sceneName}'?");
                        ImGui.Text("This action cannot be undone!");
                        
                        if (ImGui.Button("Delete"))
                        {
                            _sceneManager.DeleteScene(scenePath);
                            ImGui.CloseCurrentPopup();
                        }
                        
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        
                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.Separator();

            // Project operations
            ImGui.Text("Project Operations");
            if (ImGui.Button("Build Project"))
            {
                var result = _projectManager.CompileProject();
                if (result.Success)
                {
                    System.Console.WriteLine("Build successful");
                }
                else
                {
                    System.Console.WriteLine($"Build failed: {result.ErrorMessage}");
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Reload Assembly"))
            {
                _projectManager.ReloadAssembly();
            }

            // Display build status
            if (project.IsLoaded)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), "Assembly Loaded");
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Assembly Not Loaded");
            }

            ImGui.End();
        }
    }
}

