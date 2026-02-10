using Astora.Editor.Project;
using Astora.Editor.Utils;
using Astora.Editor.Core;
using ImGuiNET;
using System.Numerics;

namespace Astora.Editor.UI
{
    /// <summary>
    /// 资源面板 - 双视图模式（列表/网格）、面包屑导航、搜索过滤、文件类型图标
    /// </summary>
    public class AssetPanel
    {
        private readonly ProjectManager _projectManager;
        private readonly IEditorContext _ctx;

        // 视图状态
        private bool _isGridView = false;
        private float _gridIconSize = 64f;

        // 导航
        private string _currentDirectory = string.Empty;
        private readonly Stack<string> _navigationHistory = new();

        // 展开状态（树形视图用）
        private readonly HashSet<string> _expandedFolders = new();

        // 搜索/过滤
        private string _searchQuery = string.Empty;
        private FileTypeFilter _activeFilter = FileTypeFilter.All;

        // 选中
        private string? _selectedFilePath;

        // 文件缓存（避免每帧扫描磁盘）
        private string[]? _cachedDirs;
        private string[]? _cachedFiles;
        private string? _cachedDirPath;

        private readonly HashSet<string> _skipFolders = new()
        {
            "bin", "obj", ".vs", ".idea", ".vscode", ".git"
        };

        // 文件类型定义
        private static readonly Dictionary<string, (string Tag, Vector4 Color)> FileTypeInfo = new()
        {
            { ".cs",    ("[C#]",  new Vector4(0.35f, 0.80f, 0.35f, 1f)) },
            { ".scene", ("[SCN]", new Vector4(0.55f, 0.75f, 1.00f, 1f)) },
            { ".png",   ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".jpg",   ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".jpeg",  ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".bmp",   ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".gif",   ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".tga",   ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".dds",   ("[IMG]", new Vector4(0.75f, 0.50f, 0.85f, 1f)) },
            { ".yaml",  ("[CFG]", new Vector4(0.95f, 0.80f, 0.30f, 1f)) },
            { ".yml",   ("[CFG]", new Vector4(0.95f, 0.80f, 0.30f, 1f)) },
            { ".json",  ("[CFG]", new Vector4(0.95f, 0.80f, 0.30f, 1f)) },
            { ".csproj",("[PRJ]", new Vector4(0.55f, 0.75f, 1.00f, 1f)) },
            { ".sln",   ("[SLN]", new Vector4(0.55f, 0.75f, 1.00f, 1f)) },
            { ".fx",    ("[SHD]", new Vector4(1.00f, 0.55f, 0.35f, 1f)) },
            { ".fxc",   ("[SHD]", new Vector4(1.00f, 0.55f, 0.35f, 1f)) },
            { ".ttf",   ("[FNT]", new Vector4(0.70f, 0.70f, 0.70f, 1f)) },
            { ".ttc",   ("[FNT]", new Vector4(0.70f, 0.70f, 0.70f, 1f)) },
        };

        private enum FileTypeFilter
        {
            All,
            Scenes,
            Images,
            Scripts,
            Config
        }

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
                ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), "No project opened");
                ImGui.End();
                return;
            }

            var project = _projectManager.CurrentProject;
            if (project == null)
            {
                ImGui.End();
                return;
            }

            // 初始化当前目录
            if (string.IsNullOrEmpty(_currentDirectory))
                _currentDirectory = project.ProjectRoot;

            // === 工具栏 ===
            RenderToolbar();

            // === 面包屑导航 ===
            RenderBreadcrumb(project.ProjectRoot);

            ImGui.Separator();

            // === 内容区域 ===
            ImGui.BeginChild("AssetContent", new Vector2(0, -24), ImGuiChildFlags.None);

            if (_isGridView)
                RenderGridView();
            else
                RenderListView(project.ProjectRoot);

            ImGui.EndChild();

            // === 底部状态栏 ===
            RenderStatusBar();

            ImGui.End();
        }

        /// <summary>
        /// 工具栏：搜索 + 过滤 + 视图切换
        /// </summary>
        private void RenderToolbar()
        {
            // 搜索框
            float filterBtnWidth = 200;
            float toggleWidth = 50;
            float searchWidth = ImGui.GetContentRegionAvail().X - filterBtnWidth - toggleWidth - 16;
            if (searchWidth < 80) searchWidth = 80;

            ImGui.SetNextItemWidth(searchWidth);
            ImGui.InputTextWithHint("##AssetSearch", "Search files...", ref _searchQuery, 256);

            ImGui.SameLine();

            // 类型过滤按钮
            RenderFilterButton("All", FileTypeFilter.All);
            ImGui.SameLine(0, 2);
            RenderFilterButton("SCN", FileTypeFilter.Scenes);
            ImGui.SameLine(0, 2);
            RenderFilterButton("IMG", FileTypeFilter.Images);
            ImGui.SameLine(0, 2);
            RenderFilterButton("C#", FileTypeFilter.Scripts);

            ImGui.SameLine();

            // 视图切换
            if (ImGui.Button(_isGridView ? "=" : "#", new Vector2(24, 0)))
                _isGridView = !_isGridView;
        }

        private void RenderFilterButton(string label, FileTypeFilter filter)
        {
            bool isActive = _activeFilter == filter;
            if (isActive)
                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiStyleManager.GetAccentDarkColor());

            if (ImGui.SmallButton(label))
            {
                _activeFilter = filter;
                InvalidateCache();
            }

            if (isActive)
                ImGui.PopStyleColor();
        }

        /// <summary>
        /// 面包屑导航
        /// </summary>
        private void RenderBreadcrumb(string projectRoot)
        {
            if (string.IsNullOrEmpty(_currentDirectory))
                return;

            var relativePath = Path.GetRelativePath(projectRoot, _currentDirectory);
            var segments = relativePath == "." ? new[] { Path.GetFileName(projectRoot) }
                : new[] { Path.GetFileName(projectRoot) }.Concat(relativePath.Split(Path.DirectorySeparatorChar)).ToArray();

            // 构建每段对应的完整路径
            string accumulated = projectRoot;
            for (int i = 0; i < segments.Length; i++)
            {
                if (i > 0)
                {
                    ImGui.SameLine(0, 2);
                    ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(), ">");
                    ImGui.SameLine(0, 2);
                    accumulated = Path.Combine(accumulated, segments[i]);
                }

                string segPath = accumulated;
                bool isLast = (i == segments.Length - 1);

                if (isLast)
                {
                    ImGui.TextColored(ImGuiStyleManager.GetTextColor(), segments[i]);
                }
                else
                {
                    if (ImGui.SmallButton(segments[i]))
                    {
                        NavigateTo(segPath);
                    }
                }
            }
        }

        private void NavigateTo(string path)
        {
            if (Directory.Exists(path) && path != _currentDirectory)
            {
                _navigationHistory.Push(_currentDirectory);
                _currentDirectory = path;
                InvalidateCache();
            }
        }

        /// <summary>
        /// 列表视图（树形，与旧版类似但带图标和过滤）
        /// </summary>
        private void RenderListView(string basePath)
        {
            if (!Directory.Exists(_currentDirectory))
                return;

            var (dirs, files) = GetCachedContents(_currentDirectory);

            // 上级目录按钮
            if (_currentDirectory != basePath)
            {
                if (ImGui.Selectable("..  [UP]", false))
                {
                    var parent = Path.GetDirectoryName(_currentDirectory);
                    if (!string.IsNullOrEmpty(parent))
                        NavigateTo(parent);
                }
            }

            // 文件夹
            foreach (var dir in dirs)
            {
                var dirName = Path.GetFileName(dir);
                ImGui.TextColored(new Vector4(0.95f, 0.80f, 0.30f, 1f), "[DIR]");
                ImGui.SameLine(0, 4);

                if (ImGui.Selectable(dirName, false))
                {
                    NavigateTo(dir);
                }
            }

            // 文件
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var ext = Path.GetExtension(file).ToLower();

                if (!PassesFilter(ext))
                    continue;

                // 搜索过滤
                if (!string.IsNullOrWhiteSpace(_searchQuery) &&
                    !fileName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
                    continue;

                // 类型图标
                if (FileTypeInfo.TryGetValue(ext, out var info))
                {
                    ImGui.TextColored(info.Color, info.Tag);
                    ImGui.SameLine(0, 4);
                }

                bool isSelected = _selectedFilePath == file;
                if (ImGui.Selectable(fileName, isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedFilePath = file;
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        HandleFileDoubleClick(file, ext);
                }

                // 图片文件拖拽
                if (IsImageFile(ext))
                {
                    if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                    {
                        var pathBytes = System.Text.Encoding.UTF8.GetBytes(file);
                        unsafe
                        {
                            fixed (byte* ptr = pathBytes)
                            {
                                ImGui.SetDragDropPayload("TEXTURE_FILE_PATH", new IntPtr(ptr), (uint)pathBytes.Length);
                            }
                        }
                        ImGui.Text($"Drag: {fileName}");
                        ImGui.EndDragDropSource();
                    }
                }

                // 右键菜单
                if (ImGui.BeginPopupContextItem($"##asset_{file}"))
                {
                    if (ImGui.MenuItem("Open"))
                        HandleFileDoubleClick(file, ext);
                    if (ImGui.MenuItem("Show in Explorer"))
                        FileOperations.ShowInExplorer(file);
                    if (ImGui.MenuItem("Copy Path"))
                        FileOperations.CopyPathToClipboard(file);
                    ImGui.EndPopup();
                }
            }
        }

        /// <summary>
        /// 网格视图
        /// </summary>
        private void RenderGridView()
        {
            if (!Directory.Exists(_currentDirectory))
                return;

            var (dirs, files) = GetCachedContents(_currentDirectory);
            var avail = ImGui.GetContentRegionAvail();
            float cellSize = _gridIconSize + 16;
            int columns = Math.Max(1, (int)(avail.X / cellSize));

            int idx = 0;

            // 上级目录
            if (_navigationHistory.Count > 0)
            {
                RenderGridCell("..", "[UP]", new Vector4(0.6f, 0.6f, 0.6f, 1f), cellSize, () =>
                {
                    var parent = Path.GetDirectoryName(_currentDirectory);
                    if (!string.IsNullOrEmpty(parent))
                        NavigateTo(parent);
                }, singleClickActivate: true);
                idx++;
                if (idx % columns != 0) ImGui.SameLine();
            }

            // 文件夹
            foreach (var dir in dirs)
            {
                var dirName = Path.GetFileName(dir);
                RenderGridCell(dirName, "[DIR]", new Vector4(0.95f, 0.80f, 0.30f, 1f), cellSize, () => NavigateTo(dir), singleClickActivate: true);
                idx++;
                if (idx % columns != 0) ImGui.SameLine();
            }

            // 文件
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var ext = Path.GetExtension(file).ToLower();

                if (!PassesFilter(ext))
                    continue;
                if (!string.IsNullOrWhiteSpace(_searchQuery) &&
                    !fileName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
                    continue;

                var (tag, color) = FileTypeInfo.TryGetValue(ext, out var finfo)
                    ? finfo
                    : ("[?]", new Vector4(0.6f, 0.6f, 0.6f, 1f));

                var filePath = file;
                RenderGridCell(fileName, tag, color, cellSize, () =>
                {
                    _selectedFilePath = filePath;
                    HandleFileDoubleClick(filePath, ext);
                });
                idx++;
                if (idx % columns != 0) ImGui.SameLine();
            }
        }

        private void RenderGridCell(string label, string tag, Vector4 tagColor, float cellSize, Action onClick, bool singleClickActivate = false)
        {
            ImGui.BeginGroup();

            // 标签（居中）— 用 Indent 代替 SetCursorPosX
            var tagSize = ImGui.CalcTextSize(tag);
            float tagPad = Math.Max(0, (cellSize - tagSize.X) * 0.5f);
            ImGui.Indent(tagPad);
            ImGui.TextColored(tagColor, tag);
            ImGui.Unindent(tagPad);

            // 文件名（截断显示）
            var displayName = label.Length > 10 ? label[..9] + "..." : label;
            var nameSize = ImGui.CalcTextSize(displayName);
            float namePad = Math.Max(0, (cellSize - nameSize.X) * 0.5f);
            ImGui.Indent(namePad);
            ImGui.Text(displayName);
            ImGui.Unindent(namePad);

            // 确保 group 至少占满 cellSize 宽度
            ImGui.Dummy(new Vector2(cellSize, 0));

            ImGui.EndGroup();

            if (ImGui.IsItemHovered())
            {
                if (singleClickActivate && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    onClick();
                else if (!singleClickActivate && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    onClick();
            }
        }

        /// <summary>
        /// 底部状态栏
        /// </summary>
        private void RenderStatusBar()
        {
            var (dirs, files) = GetCachedContents(_currentDirectory);
            int dirCount = dirs?.Length ?? 0;
            int fileCount = files?.Length ?? 0;

            ImGui.Separator();
            ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(),
                $"{dirCount} folders, {fileCount} files");

            if (_selectedFilePath != null)
            {
                ImGui.SameLine();
                ImGui.TextColored(ImGuiStyleManager.GetTextDisabledColor(),
                    $"  |  {Path.GetFileName(_selectedFilePath)}");
            }
        }

        // === 辅助方法 ===

        private (string[] dirs, string[] files) GetCachedContents(string directory)
        {
            if (_cachedDirPath == directory && _cachedDirs != null && _cachedFiles != null)
                return (_cachedDirs, _cachedFiles);

            try
            {
                _cachedDirs = Directory.GetDirectories(directory)
                    .Where(d => !_skipFolders.Contains(Path.GetFileName(d).ToLower()))
                    .OrderBy(d => Path.GetFileName(d))
                    .ToArray();

                _cachedFiles = Directory.GetFiles(directory)
                    .OrderBy(f => Path.GetFileName(f))
                    .ToArray();

                _cachedDirPath = directory;
            }
            catch
            {
                _cachedDirs = Array.Empty<string>();
                _cachedFiles = Array.Empty<string>();
            }

            return (_cachedDirs, _cachedFiles);
        }

        private void InvalidateCache()
        {
            _cachedDirPath = null;
            _cachedDirs = null;
            _cachedFiles = null;
        }

        private bool PassesFilter(string ext)
        {
            return _activeFilter switch
            {
                FileTypeFilter.All => true,
                FileTypeFilter.Scenes => ext == ".scene",
                FileTypeFilter.Images => IsImageFile(ext),
                FileTypeFilter.Scripts => ext == ".cs",
                FileTypeFilter.Config => ext is ".yaml" or ".yml" or ".json",
                _ => true
            };
        }

        private bool IsImageFile(string extension)
        {
            return extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tga" or ".dds";
        }

        private void HandleFileDoubleClick(string filePath, string extension)
        {
            switch (extension)
            {
                case ".scene.cs":
                case ".cs":
                    // Check if this is a scene file - try to find matching IScene
                    var sceneName = Path.GetFileNameWithoutExtension(filePath);
                    if (sceneName.EndsWith(".scene"))
                        sceneName = Path.GetFileNameWithoutExtension(sceneName);
                    var sceneInfo = _ctx.ProjectService.SceneManager.FindScene(sceneName);
                    if (sceneInfo != null)
                    {
                        _ctx.Actions.LoadScene(sceneInfo);
                        break;
                    }
                    FileOperations.OpenFileInExternalEditor(filePath);
                    break;
                default:
                    FileOperations.OpenFileInExternalEditor(filePath);
                    break;
            }
        }
    }
}
