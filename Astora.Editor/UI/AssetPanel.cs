using Astora.Editor.Project;
using Astora.Editor.Utils;
using Astora.Editor.Core;
using ImGuiNET;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 资源面板 - 显示项目文件结构
    /// </summary>
    public class AssetPanel
    {
        private readonly ProjectManager _projectManager;
        private readonly IEditorContext _ctx;
        private readonly HashSet<string> _expandedFolders = new();
        private readonly HashSet<string> _skipFolders = new()
        {
            "bin", "obj", ".vs", ".idea", ".vscode"
        };

        public AssetPanel(ProjectManager projectManager, IEditorContext ctx)
        {
            _projectManager = projectManager;
            _ctx = ctx;
        }

        public void Render()
        {
            ImGui.Begin("Asset");

            if (!_projectManager.HasProject)
            {
                ImGui.Text("No project opened");
                ImGui.End();
                return;
            }

            var project = _projectManager.CurrentProject;
            if (project == null)
            {
                ImGui.End();
                return;
            }

            // 刷新按钮
            if (ImGui.Button("Refresh"))
            {
                _expandedFolders.Clear();
            }

            ImGui.Separator();

            // 显示项目文件树
            RenderFileTree(project.ProjectRoot, project.ProjectRoot);

            ImGui.End();
        }

        private void RenderFileTree(string directory, string basePath)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            try
            {
                var dirs = Directory.GetDirectories(directory)
                    .Where(d => !_skipFolders.Contains(Path.GetFileName(d).ToLower()))
                    .OrderBy(d => Path.GetFileName(d))
                    .ToArray();

                var files = Directory.GetFiles(directory)
                    .OrderBy(f => Path.GetFileName(f))
                    .ToArray();

                // 渲染文件夹
                foreach (var dir in dirs)
                {
                    var dirName = Path.GetFileName(dir);
                    var dirPath = Path.GetFullPath(dir);
                    var isExpanded = _expandedFolders.Contains(dirPath);

                    var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;
                    if (isExpanded)
                    {
                        flags |= ImGuiTreeNodeFlags.DefaultOpen;
                    }

                    var nodeOpen = ImGui.TreeNodeEx(dirName, flags);
                    
                    // 处理展开/折叠
                    if (ImGui.IsItemToggledOpen())
                    {
                        if (isExpanded)
                        {
                            _expandedFolders.Remove(dirPath);
                        }
                        else
                        {
                            _expandedFolders.Add(dirPath);
                        }
                    }

                    if (nodeOpen)
                    {
                        RenderFileTree(dir, basePath);
                        ImGui.TreePop();
                    }
                }

                // 渲染文件
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var filePath = Path.GetFullPath(file);
                    var extension = Path.GetExtension(file).ToLower();

                    // 根据文件类型显示不同的图标（使用文本表示）
                    var displayName = GetFileDisplayName(fileName, extension);
                    
                    if (ImGui.Selectable(displayName, false, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        HandleFileClick(filePath, extension);
                    }

                    // 为图片文件添加拖拽源
                    if (IsImageFile(extension))
                    {
                        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                        {
                            // 设置拖拽数据
                            var pathBytes = System.Text.Encoding.UTF8.GetBytes(filePath);
                            unsafe
                            {
                                fixed (byte* ptr = pathBytes)
                                {
                                    ImGui.SetDragDropPayload("TEXTURE_FILE_PATH", new IntPtr(ptr), (uint)pathBytes.Length);
                                }
                            }
                            
                            ImGui.Text($"拖拽纹理: {fileName}");
                            ImGui.EndDragDropSource();
                        }
                    }

                    // 双击打开
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        HandleFileDoubleClick(filePath, extension);
                    }

                    // 右键菜单
                    if (ImGui.BeginPopupContextItem($"##{filePath}"))
                    {
                        if (ImGui.MenuItem("Open"))
                        {
                            HandleFileDoubleClick(filePath, extension);
                        }

                        if (ImGui.MenuItem("Show in Explorer"))
                        {
                            FileOperations.ShowInExplorer(filePath);
                        }

                        if (ImGui.MenuItem("Copy Path"))
                        {
                            FileOperations.CopyPathToClipboard(filePath);
                        }

                        ImGui.EndPopup();
                    }
                }
            }
            catch (Exception ex)
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), $"Error: {ex.Message}");
            }
        }

        private string GetFileDisplayName(string fileName, string extension)
        {
            // 根据文件类型添加图标标识（使用文本）
            return extension switch
            {
                ".cs" => $"{fileName}",
                ".scene" => $"{fileName}",
                ".csproj" => $"{fileName}",
                ".yaml" => $"{fileName}",
                ".png" => $"{fileName}",
                ".jpg" or ".jpeg" => $"{fileName}",
                _ => $"{fileName}"
            };
        }

        private void HandleFileClick(string filePath, string extension)
        {
        }

        private void HandleFileDoubleClick(string filePath, string extension)
        {
            switch (extension)
            {
                case ".scene":
                    // 加载场景
                    _ctx.Actions.LoadScene(filePath);
                    break;

                case ".cs":
                    // 使用系统默认编辑器打开代码文件
                    FileOperations.OpenFileInExternalEditor(filePath);
                    break;

                default:
                    // 使用系统默认程序打开其他文件
                    FileOperations.OpenFileInExternalEditor(filePath);
                    break;
            }
        }
        
        private bool IsImageFile(string extension)
        {
            return extension switch
            {
                ".png" => true,
                ".jpg" => true,
                ".jpeg" => true,
                ".bmp" => true,
                ".gif" => true,
                ".tga" => true,
                ".dds" => true,
                _ => false
            };
        }
    }
}

