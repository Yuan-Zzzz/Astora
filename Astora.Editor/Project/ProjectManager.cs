using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace Astora.Editor.Project
{
    /// <summary>
    /// 项目管理器 - 负责项目加载、编译和程序集管理
    /// </summary>
    public class ProjectManager
    {
        private ProjectInfo? _currentProject;
        private AssemblyLoadContext? _assemblyContext;
        private Assembly? _loadedAssembly;

        public ProjectInfo? CurrentProject => _currentProject;
        public bool HasProject => _currentProject != null && _currentProject.IsLoaded;

        /// <summary>
        /// 加载项目
        /// </summary>
        public ProjectInfo LoadProject(string csprojPath)
        {
            if (!File.Exists(csprojPath))
            {
                throw new FileNotFoundException($"项目文件不存在: {csprojPath}");
            }

            var projectInfo = new ProjectInfo
            {
                ProjectPath = Path.GetFullPath(csprojPath),
                ProjectRoot = Path.GetDirectoryName(Path.GetFullPath(csprojPath)) ?? string.Empty
            };

            // 解析 .csproj 文件
            ParseProjectFile(projectInfo);

            _currentProject = projectInfo;
            return projectInfo;
        }

        /// <summary>
        /// 解析 .csproj 文件
        /// </summary>
        private void ParseProjectFile(ProjectInfo projectInfo)
        {
            var doc = XDocument.Load(projectInfo.ProjectPath);
            var ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

            // 获取程序集名称
            var assemblyName = doc.Descendants(ns + "AssemblyName").FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = Path.GetFileNameWithoutExtension(projectInfo.ProjectPath);
            }
            projectInfo.AssemblyName = assemblyName;

            // 获取输出路径
            var outputPath = doc.Descendants(ns + "OutputPath").FirstOrDefault()?.Value ?? "bin/Debug/net8.0";
            projectInfo.OutputPath = Path.Combine(projectInfo.ProjectRoot, outputPath);
            projectInfo.AssemblyPath = Path.Combine(projectInfo.OutputPath, $"{projectInfo.AssemblyName}.dll");
        }

        /// <summary>
        /// 编译项目
        /// </summary>
        public CompileResult CompileProject()
        {
            if (_currentProject == null)
            {
                return new CompileResult
                {
                    Success = false,
                    ErrorMessage = "没有加载的项目"
                };
            }

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build \"{_currentProject.ProjectPath}\" --no-incremental",
                    WorkingDirectory = _currentProject.ProjectRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    return new CompileResult
                    {
                        Success = false,
                        ErrorMessage = "无法启动编译进程"
                    };
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                var success = process.ExitCode == 0;

                if (success)
                {
                    // 更新程序集路径
                    _currentProject.AssemblyPath = Path.Combine(_currentProject.OutputPath, $"{_currentProject.AssemblyName}.dll");
                }

                return new CompileResult
                {
                    Success = success,
                    Output = output,
                    ErrorMessage = error,
                    AssemblyPath = _currentProject.AssemblyPath
                };
            }
            catch (Exception ex)
            {
                return new CompileResult
                {
                    Success = false,
                    ErrorMessage = $"编译失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 加载项目程序集
        /// </summary>
        public bool LoadProjectAssembly()
        {
            if (_currentProject == null)
            {
                return false;
            }

            if (!File.Exists(_currentProject.AssemblyPath))
            {
                System.Console.WriteLine($"程序集文件不存在: {_currentProject.AssemblyPath}");
                return false;
            }

            try
            {
                // 卸载旧的程序集上下文
                UnloadAssembly();

                // 创建新的可收集的程序集加载上下文
                _assemblyContext = new AssemblyLoadContext($"Project_{_currentProject.AssemblyName}", isCollectible: true);

                // 加载程序集
                _loadedAssembly = _assemblyContext.LoadFromAssemblyPath(_currentProject.AssemblyPath);

                // 确保程序集被加载到默认上下文中，以便序列化器能够找到类型
                // 注意：这会导致程序集无法完全卸载，但可以确保类型查找正常工作
                var defaultContext = AssemblyLoadContext.Default;
                try
                {
                    // 尝试从默认上下文加载（如果已存在）
                    var existingAssembly = defaultContext.Assemblies
                        .FirstOrDefault(a => a.GetName().Name == _currentProject.AssemblyName);
                    
                    if (existingAssembly == null)
                    {
                        // 如果不存在，从文件加载到默认上下文
                        defaultContext.LoadFromAssemblyPath(_currentProject.AssemblyPath);
                    }
                }
                catch
                {
                    // 忽略加载到默认上下文的错误，使用 AssemblyLoadContext 即可
                }

                _currentProject.IsLoaded = true;
                System.Console.WriteLine($"程序集加载成功: {_currentProject.AssemblyName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"加载程序集失败: {ex.Message}");
                System.Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 重新加载程序集（热重载）
        /// </summary>
        public bool ReloadAssembly()
        {
            if (_currentProject == null)
            {
                return false;
            }

            // 先编译
            var compileResult = CompileProject();
            if (!compileResult.Success)
            {
                System.Console.WriteLine($"编译失败: {compileResult.ErrorMessage}");
                return false;
            }

            // 再加载
            return LoadProjectAssembly();
        }

        /// <summary>
        /// 卸载程序集
        /// </summary>
        public void UnloadAssembly()
        {
            if (_assemblyContext != null)
            {
                _loadedAssembly = null;
                _assemblyContext.Unload();
                _assemblyContext = null;

                if (_currentProject != null)
                {
                    _currentProject.IsLoaded = false;
                }

                // 强制垃圾回收以释放程序集
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        /// <summary>
        /// 获取已加载的程序集
        /// </summary>
        public Assembly? GetLoadedAssembly()
        {
            return _loadedAssembly;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            UnloadAssembly();
            _currentProject = null;
        }
    }

    /// <summary>
    /// 编译结果
    /// </summary>
    public class CompileResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string AssemblyPath { get; set; } = string.Empty;
    }
}

