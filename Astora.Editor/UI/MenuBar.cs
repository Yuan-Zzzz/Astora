using ImGuiNET;
using System.Runtime.InteropServices;

namespace Astora.Editor.UI
{
    public class MenuBar
    {
        private Editor _editor;
        private bool _showOpenProjectDialog = false;
        private bool _showOpenSceneDialog = false;
        private bool _showSaveSceneDialog = false;
        private string _projectPathInput = string.Empty;
        private string _scenePathInput = string.Empty;
        private string _newSceneNameInput = string.Empty;
        
        public MenuBar(Editor editor)
        {
            _editor = editor;
        }
        
        public void Render()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Project...", "Ctrl+Shift+N"))
                    {
                        _editor.ShowCreateProjectDialog();
                    }
                    
                    if (ImGui.MenuItem("Open Project...", "Ctrl+O"))
                    {
                        ShowOpenProjectDialog();
                    }
                    
                    if (_editor.IsProjectLoaded)
                    {
                        ImGui.Separator();
                        if (ImGui.MenuItem("Close Project"))
                        {
                            _editor.CloseProject();
                        }
                        ImGui.Separator();
                    }
                    else
                    {
                        ImGui.Separator();
                    }
                    
                    if (_editor.IsProjectLoaded)
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
                            _editor.SaveScene();
                        }
                        
                        if (ImGui.MenuItem("Save Scene As...", "Ctrl+Shift+S"))
                        {
                            _showSaveSceneDialog = true;
                            _scenePathInput = _editor.CurrentScenePath ?? string.Empty;
                        }
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (_editor.IsProjectLoaded && ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Build Project", "Ctrl+B"))
                    {
                        var result = _editor.ProjectManager.CompileProject();
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
                        _editor.RebuildProject();
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "Ctrl+Z"))
                    {
                        // Undo operation
                    }
                    if (ImGui.MenuItem("Redo", "Ctrl+Y"))
                    {
                        // Redo operation
                    }
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Run"))
                {
                    if (ImGui.MenuItem("Play", "F5"))
                    {
                        _editor.SetPlaying(true);
                    }
                    if (ImGui.MenuItem("Pause", "F6"))
                    {
                        // Pause
                    }
                    if (ImGui.MenuItem("Stop", "Shift+F5"))
                    {
                        _editor.SetPlaying(false);
                    }
                    ImGui.EndMenu();
                }
                
                ImGui.EndMainMenuBar();
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
                    // Use Windows file dialog
                    var path = ShowOpenFileDialog("Select Project File", "*.csproj");
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
                        _editor.LoadProject(_projectPathInput);
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
                        _editor.CreateNewScene(_newSceneNameInput);
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
                
                var scenes = _editor.SceneManager.ScanScenes();
                foreach (var scenePath in scenes)
                {
                    var sceneName = _editor.SceneManager.GetSceneName(scenePath);
                    if (ImGui.Selectable(sceneName))
                    {
                        _editor.LoadScene(scenePath);
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
                        _editor.SaveScene(_scenePathInput);
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

        // Windows file dialog using native Windows API
        private string ShowOpenFileDialog(string title, string filter)
        {
            try
            {
                var ofn = new OpenFileName
                {
                    lStructSize = Marshal.SizeOf<OpenFileName>(),
                    lpstrTitle = title,
                    lpstrFilter = $"Project Files\0{filter}\0All Files\0*.*\0\0",
                    nFilterIndex = 1,
                    lpstrFile = new string('\0', 260),
                    nMaxFile = 260,
                    lpstrInitialDir = Environment.CurrentDirectory,
                    Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008 // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT | OFN_NOCHANGEDIR
                };

                if (GetOpenFileName(ref ofn))
                {
                    return ofn.lpstrFile;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing file dialog: {ex.Message}");
            }

            return string.Empty;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int FlagsEx;
        }

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);
    }
}
