namespace Astora.Editor.Project
{
    /// <summary>
    /// 最近项目信息
    /// </summary>
    public class RecentProjectInfo
    {
        public string Path { get; set; } = string.Empty;
        public DateTime LastOpened { get; set; }
    }

    /// <summary>
    /// 项目设置 - 管理最近打开的项目列表
    /// </summary>
    public static class ProjectSettings
    {
        private const int MaxRecentProjects = 10;
        private static string SettingsDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Astora"
        );
        private static string SettingsFile => Path.Combine(SettingsDirectory, "recent_projects.txt");

        /// <summary>
        /// 获取最近打开的项目列表
        /// </summary>
        public static List<RecentProjectInfo> GetRecentProjects()
        {
            var projects = new List<RecentProjectInfo>();
            
            if (!File.Exists(SettingsFile))
            {
                return projects;
            }

            try
            {
                var lines = File.ReadAllLines(SettingsFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        projects.Add(new RecentProjectInfo
                        {
                            Path = parts[0],
                            LastOpened = DateTime.TryParse(parts[1], out var date) ? date : DateTime.Now
                        });
                    }
                    else if (parts.Length == 1)
                    {
                        // 兼容旧格式（只有路径）
                        projects.Add(new RecentProjectInfo
                        {
                            Path = parts[0],
                            LastOpened = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error loading recent projects: {ex.Message}");
            }
            
            return projects;
        }

        /// <summary>
        /// 添加项目到最近列表
        /// </summary>
        public static void AddRecentProject(string projectPath)
        {
            try
            {
                if (!File.Exists(projectPath))
                {
                    return;
                }

                var recentProjects = GetRecentProjects();
                
                // 移除已存在的相同项目
                recentProjects.RemoveAll(p => p.Path.Equals(projectPath, StringComparison.OrdinalIgnoreCase));
                
                // 添加到开头
                recentProjects.Insert(0, new RecentProjectInfo
                {
                    Path = projectPath,
                    LastOpened = DateTime.Now
                });
                
                // 限制数量
                if (recentProjects.Count > MaxRecentProjects)
                {
                    recentProjects = recentProjects.Take(MaxRecentProjects).ToList();
                }
                
                // 保存
                SaveRecentProjects(recentProjects);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error adding recent project: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存最近项目列表
        /// </summary>
        private static void SaveRecentProjects(List<RecentProjectInfo> projects)
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                var lines = projects.Select(p => $"{p.Path}|{p.LastOpened:O}").ToArray();
                File.WriteAllLines(SettingsFile, lines);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error saving recent projects: {ex.Message}");
            }
        }

        /// <summary>
        /// 移除最近项目
        /// </summary>
        public static void RemoveRecentProject(string projectPath)
        {
            try
            {
                var recentProjects = GetRecentProjects();
                recentProjects.RemoveAll(p => p.Path.Equals(projectPath, StringComparison.OrdinalIgnoreCase));
                SaveRecentProjects(recentProjects);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error removing recent project: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除所有最近项目
        /// </summary>
        public static void ClearRecentProjects()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    File.Delete(SettingsFile);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error clearing recent projects: {ex.Message}");
            }
        }
    }
}

