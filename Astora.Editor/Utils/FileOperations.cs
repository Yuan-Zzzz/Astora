using System.Diagnostics;

namespace Astora.Editor.Utils
{
    /// <summary>
    /// 文件操作工具类
    /// </summary>
    public static class FileOperations
    {
        /// <summary>
        /// 使用系统默认程序打开文件
        /// </summary>
        public static bool OpenFileInExternalEditor(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error opening file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用指定的编辑器打开文件
        /// </summary>
        public static bool OpenFileInEditor(string filePath, string editorPath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            if (!File.Exists(editorPath))
            {
                // 如果编辑器路径不存在，尝试使用系统默认程序
                return OpenFileInExternalEditor(filePath);
            }

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = editorPath,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error opening file in editor: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 在文件管理器中显示文件
        /// </summary>
        public static bool ShowInExplorer(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return false;
            }

            try
            {
                var directory = File.Exists(filePath) ? Path.GetDirectoryName(filePath) : filePath;
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = File.Exists(filePath) 
                        ? $"/select,\"{filePath}\"" 
                        : $"\"{directory}\"",
                    UseShellExecute = false
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing in explorer: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 复制文件路径到剪贴板（简化实现）
        /// </summary>
        public static void CopyPathToClipboard(string filePath)
        {
            // 简化实现，实际可以使用 System.Windows.Forms.Clipboard
            System.Console.WriteLine($"Path: {filePath}");
        }
    }
}

