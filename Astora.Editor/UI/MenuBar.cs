using Astora.Editor.Core;
using Astora.Editor.Utils;
using ImGuiNET;

namespace Astora.Editor.UI
{
    public class MenuBar
    {
        private readonly IEditorContext _ctx;
        private readonly Action _showCreateProjectDialog;
        private bool _showOpenProjectDialog = false;
        private bool _showOpenSceneDialog = false;
        private bool _showSaveSceneDialog = false;
        private string _projectPathInput = string.Empty;
        private string _scenePathInput = string.Empty;
        private string _newSceneNameInput = string.Empty;
        
        public MenuBar(IEditorContext ctx, Action showCreateProjectDialog)
        {
            _ctx = ctx;
            _showCreateProjectDialog = showCreateProjectDialog;
        }
        
        public void Render()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Project...", "Ctrl+Shift+N"))
                    {
                        _showCreateProjectDialog();
                    }
                    
                    if (ImGui.MenuItem("Open Project...", "Ctrl+O"))
                    {
                        ShowOpenProjectDialog();
                    }
                    
                    if (_ctx.EditorService.State.IsProjectLoaded)
                    {
                        ImGui.Separator();
                        if (ImGui.MenuItem("Close Project"))
                        {
                            _ctx.Actions.CloseProject();
                        }
                        ImGui.Separator();
                    }
                    else
                    {
                        ImGui.Separator();
                    }
                    
                    if (_ctx.EditorService.State.IsProjectLoaded)
                    {
                        if (ImGui.MenuItem("New Scene", "Ctrl+N"))
                        {
                            _newSceneNameInput = "NewScene";
                            ImGui.OpenPopup("NewSceneDialog");
                        }
                        
                        if (ImGui.MenuItem("Open Scene...", "Ctrl+Shift+O"))
                        {
                            _showOpenSceneDialog = true;
                            _scenePathInput = string.Empty;
                        }
                        
                        if (ImGui.MenuItem("Save Scene", "Ctrl+S"))
                        {
                            _ctx.Actions.SaveScene();
                        }
                        
                        if (ImGui.MenuItem("Save Scene As...", "Ctrl+Shift+S"))
                        {
                            _showSaveSceneDialog = true;
                            _scenePathInput = _ctx.EditorService.State.CurrentScenePath ?? string.Empty;
                        }
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (_ctx.EditorService.State.IsProjectLoaded && ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Build Project", "Ctrl+B"))
                    {
                        var result = _ctx.ProjectService.ProjectManager.CompileProject();
                        if (result.Success)
                        {
                            System.Console.WriteLine("Build successful");
                        }
                        else
                        {
                            System.Console.WriteLine($"Build failed: {result.ErrorMessage}");
                        }
                    }
                    
                    if (ImGui.MenuItem("Reload Assembly", "Ctrl+R"))
                    {
                        _ctx.Actions.RebuildProject();
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "Ctrl+Z", false, _ctx.Commands.CanUndo))
                        _ctx.Commands.TryUndo();

                    if (ImGui.MenuItem("Redo", "Ctrl+Y", false, _ctx.Commands.CanRedo))
                        _ctx.Commands.TryRedo();
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Run"))
                {
                    if (ImGui.MenuItem("Play", "F5"))
                    {
                        _ctx.Actions.SetPlaying(true);
                    }
                    if (ImGui.MenuItem("Pause", "F6"))
                    {
                        // Pause
                    }
                    if (ImGui.MenuItem("Stop", "Shift+F5"))
                    {
                        _ctx.Actions.SetPlaying(false);
                    }
                    ImGui.EndMenu();
                }
                
                ImGui.EndMainMenuBar();
            }

            // 全局快捷键（避免打字时触发）
            var io = ImGui.GetIO();
            if (!io.WantTextInput)
            {
                // Ctrl+Z / Ctrl+Y
                if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.Z))
                    _ctx.Commands.TryUndo();
                if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.Y))
                    _ctx.Commands.TryRedo();

                // Ctrl+Shift+Z（常见 redo）
                if (io.KeyCtrl && io.KeyShift && ImGui.IsKeyPressed(ImGuiKey.Z))
                    _ctx.Commands.TryRedo();
            }

            // 打开项目对话框
            if (_showOpenProjectDialog)
            {
                ImGui.OpenPopup("Open Project");
                _showOpenProjectDialog = false;
            }

            if (ImGui.BeginPopupModal("Open Project"))
            {
                ImGui.Text("Enter project file path (.csproj):");
                ImGui.InputText("##ProjectPath", ref _projectPathInput, 512);
                
                if (ImGui.Button("Browse..."))
                {
                    var path = FileDialog.ShowOpenFileDialog("Select Project File", "*.csproj", Environment.CurrentDirectory);
                    if (!string.IsNullOrEmpty(path))
                    {
                        _projectPathInput = path;
                    }
                }
                
                ImGui.Separator();
                
                if (ImGui.Button("Open"))
                {
                    if (!string.IsNullOrEmpty(_projectPathInput) && File.Exists(_projectPathInput))
                    {
                        _ctx.Actions.LoadProject(_projectPathInput);
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

            // New scene dialog
            if (ImGui.BeginPopupModal("NewSceneDialog"))
            {
                ImGui.Text("Enter scene name:");
                ImGui.InputText("##NewSceneName", ref _newSceneNameInput, 256);
                
                if (ImGui.Button("Create"))
                {
                    if (!string.IsNullOrWhiteSpace(_newSceneNameInput))
                    {
                        _ctx.Actions.CreateNewScene(_newSceneNameInput);
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

            // Open scene dialog
            if (_showOpenSceneDialog)
            {
                ImGui.OpenPopup("Open Scene");
                _showOpenSceneDialog = false;
            }

            if (ImGui.BeginPopupModal("Open Scene"))
            {
                ImGui.Text("Select scene file:");
                
                var scenes = _ctx.ProjectService.SceneManager.ScanScenes();
                foreach (var scenePath in scenes)
                {
                    var sceneName = _ctx.ProjectService.SceneManager.GetSceneName(scenePath);
                    if (ImGui.Selectable(sceneName))
                    {
                        _ctx.Actions.LoadScene(scenePath);
                        ImGui.CloseCurrentPopup();
                    }
                }
                
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }

            // Save scene dialog
            if (_showSaveSceneDialog)
            {
                ImGui.OpenPopup("Save Scene As");
                _showSaveSceneDialog = false;
            }

            if (ImGui.BeginPopupModal("Save Scene As"))
            {
                ImGui.Text("Enter scene file path:");
                ImGui.InputText("##ScenePath", ref _scenePathInput, 512);
                
                if (ImGui.Button("Save"))
                {
                    if (!string.IsNullOrEmpty(_scenePathInput))
                    {
                        _ctx.Actions.SaveScene(_scenePathInput);
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
        }

        /// <summary>
        /// 显示打开项目对话框（公共方法供 Editor 调用）
        /// </summary>
        public void ShowOpenProjectDialog()
        {
            _showOpenProjectDialog = true;
            _projectPathInput = string.Empty;
        }
    }
}
