using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Utils;

namespace Astora.Editor.Project
{
    /// <summary>
    /// 场景管理器 - 负责场景文件的扫描、加载、保存
    /// </summary>
    public class SceneManager
    {
        private readonly ProjectManager _projectManager;
        private readonly ISceneSerializer _serializer;
        private string _scenesDirectory = string.Empty;

        public SceneManager(ProjectManager projectManager)
        {
            _projectManager = projectManager;
            _serializer = Engine.Serializer;
        }

        /// <summary>
        /// 初始化场景目录
        /// </summary>
        public void Initialize()
        {
            if (_projectManager.CurrentProject == null)
            {
                return;
            }

            _scenesDirectory = Path.Combine(_projectManager.CurrentProject.ProjectRoot, "Scenes");
            
            // 如果目录不存在，创建它
            if (!Directory.Exists(_scenesDirectory))
            {
                Directory.CreateDirectory(_scenesDirectory);
            }
        }

        /// <summary>
        /// 扫描场景文件
        /// </summary>
        public List<string> ScanScenes()
        {
            if (string.IsNullOrEmpty(_scenesDirectory) || !Directory.Exists(_scenesDirectory))
            {
                return new List<string>();
            }

            var sceneFiles = Directory.GetFiles(_scenesDirectory, $"*{_serializer.GetExtension()}", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFullPath)
                .ToList();

            if (_projectManager.CurrentProject != null)
            {
                _projectManager.CurrentProject.Scenes = sceneFiles;
            }

            return sceneFiles;
        }

        /// <summary>
        /// 获取场景名称（不含路径和扩展名）
        /// </summary>
        public string GetSceneName(string scenePath)
        {
            return Path.GetFileNameWithoutExtension(scenePath);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public Node? LoadScene(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                System.Console.WriteLine($"场景文件不存在: {scenePath}");
                return null;
            }

            try
            {
                return _serializer.Load(scenePath);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"加载场景失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 保存场景
        /// </summary>
        public bool SaveScene(string scenePath, Node root)
        {
            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(scenePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _serializer.Save(root, scenePath);
                
                // 更新场景列表
                ScanScenes();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"保存场景失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建新场景
        /// </summary>
        public string CreateNewScene(string sceneName)
        {
            if (string.IsNullOrEmpty(_scenesDirectory))
            {
                Initialize();
            }

            // 确保场景名称合法
            sceneName = SanitizeSceneName(sceneName);
            
            var scenePath = Path.Combine(_scenesDirectory, $"{sceneName}{_serializer.GetExtension()}");

            // 如果文件已存在，添加数字后缀
            int counter = 1;
            var originalPath = scenePath;
            while (File.Exists(scenePath))
            {
                scenePath = Path.Combine(_scenesDirectory, $"{sceneName}_{counter}{_serializer.GetExtension()}");
                counter++;
            }

            // 创建空场景（根节点）
            var rootNode = new Node(sceneName);
            SaveScene(scenePath, rootNode);

            return scenePath;
        }

        /// <summary>
        /// 删除场景
        /// </summary>
        public bool DeleteScene(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                return false;
            }

            try
            {
                File.Delete(scenePath);
                ScanScenes();
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"删除场景失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 重命名场景
        /// </summary>
        public bool RenameScene(string oldPath, string newName)
        {
            if (!File.Exists(oldPath))
            {
                return false;
            }

            newName = SanitizeSceneName(newName);
            var newPath = Path.Combine(Path.GetDirectoryName(oldPath) ?? _scenesDirectory, 
                $"{newName}{_serializer.GetExtension()}");

            if (File.Exists(newPath))
            {
                System.Console.WriteLine($"场景文件已存在: {newPath}");
                return false;
            }

            try
            {
                File.Move(oldPath, newPath);
                ScanScenes();
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"重命名场景失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取场景的完整路径
        /// </summary>
        public string GetScenePath(string sceneName)
        {
            return Path.Combine(_scenesDirectory, $"{sceneName}{_serializer.GetExtension()}");
        }

        /// <summary>
        /// 清理场景名称（移除非法字符）
        /// </summary>
        private string SanitizeSceneName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                name = name.Replace(c, '_');
            }
            return name.Trim();
        }

        /// <summary>
        /// 获取场景目录路径
        /// </summary>
        public string GetScenesDirectory()
        {
            return _scenesDirectory;
        }
    }
}

