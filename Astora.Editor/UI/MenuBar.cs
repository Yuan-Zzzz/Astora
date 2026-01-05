using ImGuiNET;

namespace Astora.Editor.UI
{
    public class MenuBar
    {
        private Editor _editor;
        
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
                    if (ImGui.MenuItem("New Scene"))
                    {
                        // Create new scene
                    }
                    if (ImGui.MenuItem("Open Scene"))
                    {
                        // Open scene file
                    }
                    if (ImGui.MenuItem("Save Scene"))
                    {
                        // Save scene
                    }
                    if (ImGui.MenuItem("Save As"))
                    {
                        // Save as
                    }
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo"))
                    {
                        // Undo operation
                    }
                    if (ImGui.MenuItem("Redo"))
                    {
                        // Redo operation
                    }
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Run"))
                {
                    if (ImGui.MenuItem("Play"))
                    {
                        _editor.SetPlaying(true);
                    }
                    if (ImGui.MenuItem("Pause"))
                    {
                        // Pause
                    }
                    if (ImGui.MenuItem("Stop"))
                    {
                        _editor.SetPlaying(false);
                    }
                    ImGui.EndMenu();
                }
                
                ImGui.EndMainMenuBar();
            }
        }
    }
}