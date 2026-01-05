namespace Astora.Editor.Utils
{
    /// <summary>
    /// 布局管理器 - 管理编辑器窗口布局
    /// </summary>
    public static class LayoutManager
    {
        private static string SettingsDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Astora"
        );
        private static string LayoutFile => Path.Combine(SettingsDirectory, "editor_layout.ini");

        /// <summary>
        /// 保存当前布局
        /// </summary>
        public static void SaveLayout()
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                // ImGui 会自动保存布局到 imgui.ini，但我们也可以保存自定义布局信息
                // 这里主要依赖 ImGui 的自动保存功能
                System.Console.WriteLine("Layout saved");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error saving layout: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载保存的布局
        /// </summary>
        public static void LoadLayout()
        {
            try
            {
                // ImGui 会自动从 imgui.ini 加载布局
                // 如果需要自定义布局加载逻辑，可以在这里实现
                System.Console.WriteLine("Layout loaded");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error loading layout: {ex.Message}");
            }
        }

        /// <summary>
        /// 重置为默认布局
        /// </summary>
        public static void ResetLayout()
        {
            try
            {
                // 删除布局文件以重置
                var imguiIni = Path.Combine(SettingsDirectory, "imgui.ini");
                if (File.Exists(imguiIni))
                {
                    File.Delete(imguiIni);
                }
                System.Console.WriteLine("Layout reset");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error resetting layout: {ex.Message}");
            }
        }
    }
}

