using Astora.Core.Project;
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
        
        // 项目设置临时变量
        private int _tempDesignWidth;
        private int _tempDesignHeight;
        private ScalingMode _tempScalingMode;
        private bool _settingsDirty = false;

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
                _editor.RebuildProject();
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
            
            ImGui.Separator();
            
            // Project Settings
            if (ImGui.CollapsingHeader("Project Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                RenderProjectSettings(project);
            }

            ImGui.End();
        }
        
        /// <summary>
        /// 渲染项目设置UI
        /// </summary>
        private void RenderProjectSettings(ProjectInfo project)
        {
            var config = project.GameConfig;
            
            // 初始化临时变量（如果未初始化或配置已更改）
            if (!_settingsDirty)
            {
                _tempDesignWidth = config.DesignWidth;
                _tempDesignHeight = config.DesignHeight;
                _tempScalingMode = config.ScalingMode;
            }
            
            ImGui.Text("Design Resolution");
            
            // 设计分辨率宽度
            ImGui.Text("Width:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##DesignWidth", ref _tempDesignWidth, 1, 10))
            {
                if (_tempDesignWidth < 1) _tempDesignWidth = 1;
                _settingsDirty = true;
            }
            
            // 设计分辨率高度
            ImGui.Text("Height:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##DesignHeight", ref _tempDesignHeight, 1, 10))
            {
                if (_tempDesignHeight < 1) _tempDesignHeight = 1;
                _settingsDirty = true;
            }
            
            // 常用分辨率快捷按钮
            ImGui.Text("Presets:");
            if (ImGui.Button("1920x1080"))
            {
                _tempDesignWidth = 1920;
                _tempDesignHeight = 1080;
                _settingsDirty = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("1280x720"))
            {
                _tempDesignWidth = 1280;
                _tempDesignHeight = 720;
                _settingsDirty = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("800x600"))
            {
                _tempDesignWidth = 800;
                _tempDesignHeight = 600;
                _settingsDirty = true;
            }
            
            ImGui.Separator();
            
            // 缩放模式
            ImGui.Text("Scaling Mode");
            var scalingModeNames = new[] { "None", "Fit", "Fill", "Stretch", "PixelPerfect" };
            var currentModeIndex = (int)_tempScalingMode;
            
            if (ImGui.Combo("##ScalingMode", ref currentModeIndex, scalingModeNames, scalingModeNames.Length))
            {
                _tempScalingMode = (ScalingMode)currentModeIndex;
                _settingsDirty = true;
            }
            
            // 显示缩放模式说明
            var modeDescription = _tempScalingMode switch
            {
                ScalingMode.None => "不缩放，1:1显示",
                ScalingMode.Fit => "保持宽高比，完整显示（可能有黑边）",
                ScalingMode.Fill => "保持宽高比，填满屏幕（可能裁剪）",
                ScalingMode.Stretch => "拉伸填满屏幕（可能变形）",
                ScalingMode.PixelPerfect => "像素完美缩放（整数倍缩放）",
                _ => ""
            };
            ImGui.TextDisabled(modeDescription);
            
            ImGui.Separator();
            
            // 保存按钮
            if (_settingsDirty)
            {
                if (ImGui.Button("Save Settings"))
                {
                    config.DesignWidth = _tempDesignWidth;
                    config.DesignHeight = _tempDesignHeight;
                    config.ScalingMode = _tempScalingMode;
                    
                    if (_projectManager.SaveProjectConfig(project))
                    {
                        _settingsDirty = false;
                        System.Console.WriteLine("项目设置已保存");
                    }
                    else
                    {
                        System.Console.WriteLine("保存项目设置失败");
                    }
                }
                
                ImGui.SameLine();
                if (ImGui.Button("Reset"))
                {
                    _tempDesignWidth = config.DesignWidth;
                    _tempDesignHeight = config.DesignHeight;
                    _tempScalingMode = config.ScalingMode;
                    _settingsDirty = false;
                }
            }
        }
    }
}

