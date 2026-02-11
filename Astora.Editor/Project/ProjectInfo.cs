using Astora.Core.Project;

namespace Astora.Editor.Project
{
    /// <summary>
    /// 项目信息
    /// </summary>
    public class ProjectInfo
    {
        public string ProjectPath { get; set; } = string.Empty;
        public string ProjectRoot { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string AssemblyPath { get; set; } = string.Empty;
        public string AssemblyName { get; set; } = string.Empty;
        public List<string> Scenes { get; set; } = new();
        public bool IsLoaded { get; set; }
        
        /// <summary>
        /// 游戏项目配置
        /// </summary>
        public GameProjectConfig GameConfig { get; set; } = GameProjectConfig.CreateDefault();

        /// <summary>
        /// IGameRuntime 实现类型（从项目程序集扫描得到，用于 Editor 播放时驱动同一套游戏逻辑）
        /// </summary>
        public Type? GameRuntimeType { get; set; }
    }
}

