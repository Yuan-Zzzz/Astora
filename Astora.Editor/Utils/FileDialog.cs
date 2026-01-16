using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Astora.Editor.Utils
{
    /// <summary>
    /// 跨平台文件对话框工具类
    /// </summary>
    public static class FileDialog
    {
        /// <summary>
        /// 显示打开文件对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="filter">文件过滤器，例如 "*.csproj" 或 "Project Files|*.csproj|All Files|*.*"</param>
        /// <param name="initialDirectory">初始目录</param>
        /// <returns>选择的文件路径，如果取消则返回空字符串</returns>
        public static string ShowOpenFileDialog(string title, string filter, string? initialDirectory = null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ShowOpenFileDialogWindows(title, filter, initialDirectory);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ShowOpenFileDialogLinux(title, filter, initialDirectory);
            }

            return string.Empty;
        }

        /// <summary>
        /// 显示保存文件对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="filter">文件过滤器</param>
        /// <param name="defaultFileName">默认文件名</param>
        /// <param name="initialDirectory">初始目录</param>
        /// <returns>保存的文件路径，如果取消则返回空字符串</returns>
        public static string ShowSaveFileDialog(string title, string filter, string? defaultFileName = null, string? initialDirectory = null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ShowSaveFileDialogWindows(title, filter, defaultFileName, initialDirectory);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ShowSaveFileDialogLinux(title, filter, defaultFileName, initialDirectory);
            }

            return string.Empty;
        }

        /// <summary>
        /// 显示文件夹选择对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="initialDirectory">初始目录</param>
        /// <returns>选择的文件夹路径，如果取消则返回空字符串</returns>
        public static string ShowFolderDialog(string title, string? initialDirectory = null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ShowFolderDialogWindows(title, initialDirectory);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ShowFolderDialogLinux(title, initialDirectory);
            }

            return string.Empty;
        }

        #region Windows Implementation

        private static string ShowOpenFileDialogWindows(string title, string filter, string? initialDirectory)
        {
            try
            {
                var ofn = new OpenFileName
                {
                    lStructSize = Marshal.SizeOf<OpenFileName>(),
                    lpstrTitle = title,
                    lpstrFilter = ConvertFilter(filter),
                    nFilterIndex = 1,
                    lpstrFile = new string('\0', 260),
                    nMaxFile = 260,
                    lpstrInitialDir = initialDirectory ?? Environment.CurrentDirectory,
                    Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008 // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR
                };

                if (GetOpenFileName(ref ofn))
                {
                    return ofn.lpstrFile.TrimEnd('\0');
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing file dialog: {ex.Message}");
            }

            return string.Empty;
        }

        private static string ShowSaveFileDialogWindows(string title, string filter, string? defaultFileName, string? initialDirectory)
        {
            try
            {
                var ofn = new OpenFileName
                {
                    lStructSize = Marshal.SizeOf<OpenFileName>(),
                    lpstrTitle = title,
                    lpstrFilter = ConvertFilter(filter),
                    nFilterIndex = 1,
                    lpstrFile = defaultFileName ?? new string('\0', 260),
                    nMaxFile = 260,
                    lpstrInitialDir = initialDirectory ?? Environment.CurrentDirectory,
                    Flags = 0x00080000 | 0x00000800 | 0x00000008 // OFN_EXPLORER | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR
                };

                if (GetSaveFileName(ref ofn))
                {
                    return ofn.lpstrFile.TrimEnd('\0');
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing save dialog: {ex.Message}");
            }

            return string.Empty;
        }

        private static string ShowFolderDialogWindows(string title, string? initialDirectory)
        {
            try
            {
                // 使用 SHBrowseForFolder API
                var bi = new BrowseInfo
                {
                    hwndOwner = IntPtr.Zero,
                    pidlRoot = IntPtr.Zero,
                    pszDisplayName = new string('\0', 260),
                    lpszTitle = title,
                    ulFlags = 0x00000040, // BIF_NEWDIALOGSTYLE
                    lpfn = IntPtr.Zero,
                    lParam = IntPtr.Zero,
                    iImage = 0
                };

                var pidl = SHBrowseForFolder(ref bi);
                if (pidl != IntPtr.Zero)
                {
                    var path = new string('\0', 260);
                    if (SHGetPathFromIDList(pidl, path))
                    {
                        CoTaskMemFree(pidl);
                        return path.TrimEnd('\0');
                    }
                    CoTaskMemFree(pidl);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing folder dialog: {ex.Message}");
            }

            return string.Empty;
        }

        private static string ConvertFilter(string filter)
        {
            // 将 "*.csproj" 转换为 "Project Files\0*.csproj\0All Files\0*.*\0\0"
            // 或将 "Project Files|*.csproj|All Files|*.*" 转换为相同格式
            if (filter.Contains("|"))
            {
                var parts = filter.Split('|');
                var result = "";
                for (int i = 0; i < parts.Length; i += 2)
                {
                    if (i + 1 < parts.Length)
                    {
                        result += parts[i] + "\0" + parts[i + 1] + "\0";
                    }
                }
                return result + "\0";
            }
            else
            {
                // 简单格式，例如 "*.csproj"
                var ext = filter;
                var name = ext.Replace("*", "").Replace(".", "").ToUpper() + " Files";
                return $"{name}\0{ext}\0All Files\0*.*\0\0";
            }
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct BrowseInfo
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetSaveFileName(ref OpenFileName ofn);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHBrowseForFolder(ref BrowseInfo lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr ptr);

        #endregion

        #region Linux Implementation

        private static string ShowOpenFileDialogLinux(string title, string filter, string? initialDirectory)
        {
            try
            {
                // 检查 zenity 是否可用
                if (!IsCommandAvailable("zenity"))
                {
                    System.Console.WriteLine("zenity is not available. Please install zenity to use file dialogs.");
                    return string.Empty;
                }

                var args = new List<string>
                {
                    "--file-selection",
                    "--title", title
                };

                if (!string.IsNullOrEmpty(initialDirectory))
                {
                    args.Add("--filename");
                    args.Add(initialDirectory);
                }

                // 处理过滤器
                if (!string.IsNullOrEmpty(filter))
                {
                    var filterParts = filter.Split('|');
                    if (filterParts.Length >= 2)
                    {
                        args.Add("--file-filter");
                        args.Add(filterParts[1]); // 例如 "*.csproj"
                    }
                }

                var result = RunCommand("zenity", args);
                return result.Trim();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing file dialog: {ex.Message}");
                return string.Empty;
            }
        }

        private static string ShowSaveFileDialogLinux(string title, string filter, string? defaultFileName, string? initialDirectory)
        {
            try
            {
                if (!IsCommandAvailable("zenity"))
                {
                    System.Console.WriteLine("zenity is not available. Please install zenity to use file dialogs.");
                    return string.Empty;
                }

                var args = new List<string>
                {
                    "--file-selection",
                    "--save",
                    "--title", title
                };

                if (!string.IsNullOrEmpty(defaultFileName))
                {
                    args.Add("--filename");
                    args.Add(defaultFileName);
                }
                else if (!string.IsNullOrEmpty(initialDirectory))
                {
                    args.Add("--filename");
                    args.Add(initialDirectory);
                }

                if (!string.IsNullOrEmpty(filter))
                {
                    var filterParts = filter.Split('|');
                    if (filterParts.Length >= 2)
                    {
                        args.Add("--file-filter");
                        args.Add(filterParts[1]);
                    }
                }

                var result = RunCommand("zenity", args);
                return result.Trim();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing save dialog: {ex.Message}");
                return string.Empty;
            }
        }

        private static string ShowFolderDialogLinux(string title, string? initialDirectory)
        {
            try
            {
                if (!IsCommandAvailable("zenity"))
                {
                    System.Console.WriteLine("zenity is not available. Please install zenity to use file dialogs.");
                    return string.Empty;
                }

                var args = new List<string>
                {
                    "--file-selection",
                    "--directory",
                    "--title", title
                };

                if (!string.IsNullOrEmpty(initialDirectory))
                {
                    args.Add("--filename");
                    args.Add(initialDirectory);
                }

                var result = RunCommand("zenity", args);
                return result.Trim();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error showing folder dialog: {ex.Message}");
                return string.Empty;
            }
        }

        private static bool IsCommandAvailable(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return false;

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static string RunCommand(string command, List<string> arguments)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // 将参数列表添加到 Arguments 中，每个参数单独添加
                foreach (var arg in arguments)
                {
                    processInfo.ArgumentList.Add(arg);
                }

                using var process = Process.Start(processInfo);
                if (process == null) return string.Empty;

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    // 用户取消对话框时，zenity 返回非零退出码（通常是 1）
                    // 不输出错误信息，因为这是正常的用户操作
                    return string.Empty;
                }

                return output;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error running command {command}: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion
    }
}
