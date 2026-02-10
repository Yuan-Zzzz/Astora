using System.Reflection;
using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.Utils;

namespace Astora.Editor.Project
{
    /// <summary>
    /// Scene info discovered from assembly IScene implementations.
    /// </summary>
    public class SceneInfo
    {
        /// <summary>Scene class name (e.g. "SampleScene")</summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>Scene path from IScene.ScenePath (e.g. "Scenes/SampleScene")</summary>
        public string ScenePath { get; set; } = string.Empty;

        /// <summary>The Type implementing IScene</summary>
        public Type SceneType { get; set; } = null!;

        /// <summary>Full path to the .scene.cs source file on disk (if known)</summary>
        public string? SourceFilePath { get; set; }
    }

    /// <summary>
    /// 场景管理器 - 负责场景发现(IScene)、加载(Build)、保存(CodeEmitter)
    /// </summary>
    public class SceneManager
    {
        private readonly ProjectManager _projectManager;
        private readonly SceneCodeEmitter _emitter = new();

        private string _scenesDirectory = string.Empty;
        private List<SceneInfo> _scenes = new();

        public SceneManager(ProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        /// <summary>
        /// All discovered scenes
        /// </summary>
        public IReadOnlyList<SceneInfo> Scenes => _scenes;

        /// <summary>
        /// 初始化场景目录
        /// </summary>
        public void Initialize()
        {
            if (_projectManager.CurrentProject == null)
                return;

            _scenesDirectory = Path.Combine(_projectManager.CurrentProject.ProjectRoot, "Scenes");

            if (!Directory.Exists(_scenesDirectory))
                Directory.CreateDirectory(_scenesDirectory);
        }

        /// <summary>
        /// 扫描程序集中的 IScene 实现（替代原来的文件扫描）
        /// </summary>
        public List<SceneInfo> ScanScenes()
        {
            _scenes.Clear();

            var assembly = _projectManager.GetLoadedAssembly();
            if (assembly == null)
            {
                // Fallback: also update old ProjectInfo.Scenes for compatibility
                if (_projectManager.CurrentProject != null)
                    _projectManager.CurrentProject.Scenes = new List<string>();
                return _scenes;
            }

            try
            {
                var sceneInterface = typeof(IScene);
                var types = assembly.GetTypes()
                    .Where(t => sceneInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in types)
                {
                    try
                    {
                        var pathProp = type.GetProperty("ScenePath", BindingFlags.Public | BindingFlags.Static);
                        var scenePath = pathProp?.GetValue(null) as string ?? $"Scenes/{type.Name}";

                        var info = new SceneInfo
                        {
                            ClassName = type.Name,
                            ScenePath = scenePath,
                            SceneType = type,
                            SourceFilePath = FindSourceFile(type.Name)
                        };

                        _scenes.Add(info);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[SceneManager] Error scanning IScene type {type.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[SceneManager] Error scanning assembly: {ex.Message}");
            }

            // Update old ProjectInfo.Scenes for compatibility with other code
            if (_projectManager.CurrentProject != null)
            {
                _projectManager.CurrentProject.Scenes = _scenes.Select(s => s.ScenePath).ToList();
            }

            System.Console.WriteLine($"[SceneManager] Found {_scenes.Count} IScene implementations");
            return _scenes;
        }

        /// <summary>
        /// 通过反射调用 IScene.Build() 加载场景
        /// </summary>
        public Node? LoadScene(SceneInfo sceneInfo)
        {
            try
            {
                System.Console.WriteLine($"[SceneManager] Loading scene via Build(): {sceneInfo.ClassName}");

                var buildMethod = sceneInfo.SceneType.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
                if (buildMethod == null)
                {
                    System.Console.WriteLine($"[SceneManager] Error: Build() method not found on {sceneInfo.ClassName}");
                    return null;
                }

                var node = (Node?)buildMethod.Invoke(null, null);

                if (node != null)
                    System.Console.WriteLine($"[SceneManager] Scene loaded: {node.Name}");
                else
                    System.Console.WriteLine("[SceneManager] Warning: Build() returned null");

                return node;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[SceneManager] Error loading scene {sceneInfo.ClassName}: {ex.Message}");
                if (ex.InnerException != null)
                    System.Console.WriteLine($"  Inner: {ex.InnerException.Message}");
                return null;
            }
        }

        /// <summary>
        /// 保存场景：生成 C# 代码写入 .scene.cs 文件
        /// </summary>
        public bool SaveScene(SceneInfo sceneInfo, Node root)
        {
            try
            {
                var namespaceName = GetDefaultNamespace();
                var className = sceneInfo.ClassName;
                var scenePath = sceneInfo.ScenePath;

                var code = _emitter.Emit(root, namespaceName, className, scenePath);

                // Determine output path
                var outputPath = sceneInfo.SourceFilePath;
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = Path.Combine(_scenesDirectory, $"{className}.scene.cs");
                    sceneInfo.SourceFilePath = outputPath;
                }

                // Ensure directory exists
                var dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(outputPath, code);
                System.Console.WriteLine($"[SceneManager] Scene saved to: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[SceneManager] Error saving scene: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 保存场景（兼容旧接口：通过路径和根节点保存，用于新建场景等）
        /// </summary>
        public bool SaveScene(string className, Node root)
        {
            var sceneInfo = _scenes.FirstOrDefault(s => s.ClassName == className);
            if (sceneInfo == null)
            {
                // Create new SceneInfo for a brand-new scene
                sceneInfo = new SceneInfo
                {
                    ClassName = className,
                    ScenePath = $"Scenes/{className}",
                    SceneType = typeof(IScene), // placeholder
                    SourceFilePath = Path.Combine(_scenesDirectory, $"{className}.scene.cs")
                };
            }

            return SaveScene(sceneInfo, root);
        }

        /// <summary>
        /// 创建新场景（生成 .scene.cs 文件）
        /// </summary>
        public SceneInfo CreateNewScene(string sceneName)
        {
            if (string.IsNullOrEmpty(_scenesDirectory))
                Initialize();

            sceneName = SanitizeSceneName(sceneName);
            var className = SanitizeClassName(sceneName);

            // Ensure unique name
            int counter = 1;
            var originalClassName = className;
            while (_scenes.Any(s => s.ClassName == className) ||
                   File.Exists(Path.Combine(_scenesDirectory, $"{className}.scene.cs")))
            {
                className = $"{originalClassName}{counter}";
                counter++;
            }

            var sceneInfo = new SceneInfo
            {
                ClassName = className,
                ScenePath = $"Scenes/{className}",
                SceneType = typeof(IScene), // placeholder until recompile
                SourceFilePath = Path.Combine(_scenesDirectory, $"{className}.scene.cs")
            };

            // Create a minimal scene with root node
            var rootNode = new Node(className);
            SaveScene(sceneInfo, rootNode);

            _scenes.Add(sceneInfo);
            return sceneInfo;
        }

        /// <summary>
        /// 删除场景
        /// </summary>
        public bool DeleteScene(SceneInfo sceneInfo)
        {
            if (sceneInfo.SourceFilePath != null && File.Exists(sceneInfo.SourceFilePath))
            {
                try
                {
                    File.Delete(sceneInfo.SourceFilePath);
                    _scenes.Remove(sceneInfo);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[SceneManager] Error deleting scene: {ex.Message}");
                    return false;
                }
            }

            _scenes.Remove(sceneInfo);
            return true;
        }

        /// <summary>
        /// 获取场景名称
        /// </summary>
        public string GetSceneName(SceneInfo sceneInfo)
        {
            return sceneInfo.ClassName;
        }

        /// <summary>
        /// 查找场景（按类名）
        /// </summary>
        public SceneInfo? FindScene(string className)
        {
            return _scenes.FirstOrDefault(s => s.ClassName == className);
        }

        /// <summary>
        /// 获取场景目录路径
        /// </summary>
        public string GetScenesDirectory()
        {
            return _scenesDirectory;
        }

        /// <summary>
        /// 获取项目默认命名空间
        /// </summary>
        private string GetDefaultNamespace()
        {
            var assemblyName = _projectManager.CurrentProject?.AssemblyName ?? "MyGame";
            return $"{assemblyName}.Scenes";
        }

        /// <summary>
        /// 尝试找到 IScene 类型对应的 .scene.cs 源文件
        /// </summary>
        private string? FindSourceFile(string className)
        {
            if (string.IsNullOrEmpty(_scenesDirectory) || !Directory.Exists(_scenesDirectory))
                return null;

            // Look for ClassName.scene.cs
            var sceneFile = Path.Combine(_scenesDirectory, $"{className}.scene.cs");
            if (File.Exists(sceneFile))
                return sceneFile;

            // Look in project root
            if (_projectManager.CurrentProject != null)
            {
                var rootFile = Path.Combine(_projectManager.CurrentProject.ProjectRoot, "Scenes", $"{className}.scene.cs");
                if (File.Exists(rootFile))
                    return rootFile;
            }

            return null;
        }

        /// <summary>
        /// 清理场景名称
        /// </summary>
        private string SanitizeSceneName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
                name = name.Replace(c, '_');
            return name.Trim();
        }

        /// <summary>
        /// 将名称转为合法的 C# 类名
        /// </summary>
        private static string SanitizeClassName(string name)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }

            if (sb.Length == 0)
                return "NewScene";

            // Ensure starts with letter or underscore
            if (char.IsDigit(sb[0]))
                sb.Insert(0, '_');

            // PascalCase the first char
            sb[0] = char.ToUpper(sb[0]);

            return sb.ToString();
        }
    }
}
