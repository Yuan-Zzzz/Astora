using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using Astora.Core.Project;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        private string? _tempAssemblyPath;

        public ProjectInfo? CurrentProject => _currentProject;
        public bool HasProject => _currentProject != null;

        /// <summary>
        /// 获取最近打开的项目列表
        /// </summary>
        public List<RecentProjectInfo> GetRecentProjects()
        {
            return ProjectSettings.GetRecentProjects();
        }

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
            
            // 加载项目配置
            LoadProjectConfig(projectInfo);

            _currentProject = projectInfo;
            
            // 添加到最近项目列表
            ProjectSettings.AddRecentProject(csprojPath);
            
            return projectInfo;
        }

        /// <summary>
        /// 创建新项目
        /// </summary>
        public ProjectInfo CreateProject(string projectName, string projectLocation, ProjectTemplateType templateType)
        {
            // 验证项目名称
            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentException("项目名称不能为空", nameof(projectName));
            }

            // 清理项目名称（移除非法字符）
            projectName = SanitizeProjectName(projectName);

            // 创建项目目录
            var projectRoot = Path.Combine(projectLocation, projectName);
            if (Directory.Exists(projectRoot))
            {
                throw new InvalidOperationException($"项目目录已存在: {projectRoot}");
            }

            Directory.CreateDirectory(projectRoot);

            // 创建项目文件
            var csprojPath = Path.Combine(projectRoot, $"{projectName}.csproj");
            var csprojContent = ProjectTemplate.GenerateCsproj(projectName, templateType);
            File.WriteAllText(csprojPath, csprojContent);

            // 创建 Scripts 文件夹
            var scriptsDir = Path.Combine(projectRoot, "Scripts");
            Directory.CreateDirectory(scriptsDir);

            // 创建 Program.cs
            var programCsPath = Path.Combine(projectRoot, "Program.cs");
            var programCsContent = ProjectTemplate.GenerateProgramCs(projectName);
            File.WriteAllText(programCsPath, programCsContent);

            // 创建 Game1.cs
            var game1CsPath = Path.Combine(scriptsDir, "Game1.cs");
            var game1CsContent = ProjectTemplate.GenerateGame1Cs(projectName, templateType);
            File.WriteAllText(game1CsPath, game1CsContent);

            // 创建 Scenes 文件夹
            var scenesDir = Path.Combine(projectRoot, "Scenes");
            Directory.CreateDirectory(scenesDir);
            
            // 创建默认项目配置文件
            CreateDefaultProjectConfig(projectRoot);

            // 加载创建的项目
            var projectInfo = LoadProject(csprojPath);
            
            return projectInfo;
        }

        /// <summary>
        /// 清理项目名称（移除非法字符）
        /// </summary>
        private string SanitizeProjectName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                name = name.Replace(c, '_');
            }
            return name.Trim();
        }

        /// <summary>
        /// 加载项目配置
        /// </summary>
        private void LoadProjectConfig(ProjectInfo projectInfo)
        {
            var configPath = Path.Combine(projectInfo.ProjectRoot, "project.yaml");
            
            if (File.Exists(configPath))
            {
                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    
                    var yaml = File.ReadAllText(configPath);
                    projectInfo.GameConfig = deserializer.Deserialize<GameProjectConfig>(yaml) 
                        ?? GameProjectConfig.CreateDefault();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"加载项目配置失败: {ex.Message}，使用默认配置");
                    projectInfo.GameConfig = GameProjectConfig.CreateDefault();
                }
            }
            else
            {
                // 如果配置文件不存在，使用默认配置
                projectInfo.GameConfig = GameProjectConfig.CreateDefault();
            }
        }
        
        /// <summary>
        /// 保存项目配置
        /// </summary>
        public bool SaveProjectConfig(ProjectInfo projectInfo)
        {
            if (projectInfo == null)
                return false;
            
            try
            {
                var configPath = Path.Combine(projectInfo.ProjectRoot, "project.yaml");
                
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                var yaml = serializer.Serialize(projectInfo.GameConfig);
                File.WriteAllText(configPath, yaml);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"保存项目配置失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 创建默认项目配置文件
        /// </summary>
        private void CreateDefaultProjectConfig(string projectRoot)
        {
            var configPath = Path.Combine(projectRoot, "project.yaml");
            
            if (!File.Exists(configPath))
            {
                var defaultConfig = GameProjectConfig.CreateDefault();
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                var yaml = serializer.Serialize(defaultConfig);
                File.WriteAllText(configPath, yaml);
            }
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
                // 如果已有程序集加载，先卸载
                if (_assemblyContext != null)
                {
                    UnloadAssembly();
                }

                // 将程序集复制到临时位置，避免锁定原始文件
                var tempDir = Path.Combine(Path.GetTempPath(), "Astora", _currentProject.AssemblyName);
                Directory.CreateDirectory(tempDir);
                
                var tempFileName = $"{_currentProject.AssemblyName}_{Guid.NewGuid():N}.dll";
                _tempAssemblyPath = Path.Combine(tempDir, tempFileName);
                
                File.Copy(_currentProject.AssemblyPath, _tempAssemblyPath, true);

                // 创建新的可收集的程序集加载上下文
                _assemblyContext = new AssemblyLoadContext($"Project_{_currentProject.AssemblyName}", isCollectible: true);

                // 从临时位置加载程序集
                _loadedAssembly = _assemblyContext.LoadFromAssemblyPath(_tempAssemblyPath);

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
                System.Console.WriteLine("没有加载的项目");
                return false;
            }

            // 卸载旧程序集
            UnloadAssembly();

            // 编译项目
            System.Console.WriteLine("开始编译项目...");
            var compileResult = CompileProject();
            if (!compileResult.Success)
            {
                System.Console.WriteLine("=== 编译失败 ===");
                if (!string.IsNullOrEmpty(compileResult.Output))
                {
                    System.Console.WriteLine("编译输出:");
                    System.Console.WriteLine(compileResult.Output);
                }
                if (!string.IsNullOrEmpty(compileResult.ErrorMessage))
                {
                    System.Console.WriteLine("错误信息:");
                    System.Console.WriteLine(compileResult.ErrorMessage);
                }
                System.Console.WriteLine("================");
                return false;
            }

            System.Console.WriteLine("编译成功");

            // 加载新程序集
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

                // 清理临时文件
                if (!string.IsNullOrEmpty(_tempAssemblyPath) && File.Exists(_tempAssemblyPath))
                {
                    try
                    {
                        File.Delete(_tempAssemblyPath);
                    }
                    catch
                    {
                        // 忽略删除失败，文件可能仍被锁定，会在下次清理时删除
                    }
                    _tempAssemblyPath = null;
                }
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
        }

        /// <summary>
        /// 清除当前项目
        /// </summary>
        public void ClearProject()
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

