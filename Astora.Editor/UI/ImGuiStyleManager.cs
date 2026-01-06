using ImGuiNET;
using System.Numerics;

namespace Astora.Editor.UI
{
    /// <summary>
    /// ImGui 样式管理器 - 负责管理编辑器 UI 的主题样式
    /// </summary>
    public static class ImGuiStyleManager
    {
        /// <summary>
        /// 应用现代深色主题（参考 Unity/Unreal Engine 风格）
        /// </summary>
        public static void ApplyModernDarkTheme()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // ============================================
            // 颜色配置（RGBA 0-1 范围）
            // ============================================
            
            // 窗口和背景颜色
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);           // #2E2E2E - 窗口背景
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);            // #262626 - 子窗口背景
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.95f);            // #333333 - 弹出窗口背景
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);            // #333333 - 标题栏背景
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);      // #404040 - 活动标题栏
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);    // #2E2E2E - 折叠标题栏
            
            // 文本颜色
            colors[(int)ImGuiCol.Text] = new Vector4(0.80f, 0.80f, 0.80f, 1.00f);              // #CCCCCC - 主文本
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);      // #808080 - 禁用文本
            
            // 强调色（选中/高亮）
            var accentColor = new Vector4(0.00f, 0.47f, 0.80f, 1.00f);                          // #007ACC - 强调色
            colors[(int)ImGuiCol.Header] = accentColor;                                          // 表头/选中项背景
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.55f, 0.90f, 1.00f);      // 悬停时的表头
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.00f, 0.40f, 0.70f, 1.00f);      // 激活时的表头
            // colors[(int)ImGuiCol.SelectionBg] = new Vector4(0.00f, 0.47f, 0.80f, 0.35f);        // 选中背景（半透明）- 某些版本不支持
            
            // 按钮颜色
            colors[(int)ImGuiCol.Button] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);              // #4D4D4D - 按钮背景
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);      // #666666 - 按钮悬停
            colors[(int)ImGuiCol.ButtonActive] = accentColor;                                    // 按钮激活
            
            // 框架和边框
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);            // #404040 - 输入框背景
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);      // 输入框悬停
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.35f, 0.35f, 0.35f, 1.00f);       // 输入框激活
            colors[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);             // #404040 - 边框
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.50f);       // 边框阴影
            
            // 分隔线
            colors[(int)ImGuiCol.Separator] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);         // #4D4D4D - 分隔线
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);   // 分隔线悬停
            colors[(int)ImGuiCol.SeparatorActive] = accentColor;                                // 分隔线激活
            
            // 滚动条
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);        // 滚动条背景
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);      // 滚动条抓取器
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f); // 滚动条抓取器悬停
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f); // 滚动条抓取器激活
            
            // 菜单
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);         // 菜单栏背景
            
            // 滑块
            colors[(int)ImGuiCol.SliderGrab] = accentColor;                                      // 滑块抓取器
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.00f, 0.40f, 0.70f, 1.00f);   // 滑块抓取器激活
            
            // 复选框和单选按钮
            colors[(int)ImGuiCol.CheckMark] = accentColor;                                        // 复选框标记
            colors[(int)ImGuiCol.PlotLines] = accentColor;                                       // 图表线条
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.00f, 0.55f, 0.90f, 1.00f);   // 图表线条悬停
            colors[(int)ImGuiCol.PlotHistogram] = accentColor;                                   // 直方图
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.00f, 0.55f, 0.90f, 1.00f); // 直方图悬停
            
            // 拖拽目标
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.00f, 0.47f, 0.80f, 0.50f);     // 拖拽目标（半透明）
            
            // 导航和聚焦（某些 ImGui.NET 版本可能不支持这些常量，如果编译错误请注释掉）
            // colors[(int)ImGuiCol.NavHighlight] = accentColor;                                    // 导航高亮
            // colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f); // 窗口导航高亮
            // colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);  // 窗口导航背景
            
            // 模态窗口（某些 ImGui.NET 版本可能不支持，如果编译错误请注释掉）
            // colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.60f);    // 模态窗口背景
            
            // 表格（某些 ImGui.NET 版本可能不支持，如果编译错误请注释掉）
            // colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);      // 表格表头
            // colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.35f, 0.35f, 0.35f, 1.00f);   // 表格强边框
            // colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);     // 表格弱边框
            // colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);         // 表格行背景（透明）
            // colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.02f);       // 表格交替行背景
            
            // ============================================
            // 间距和尺寸配置
            // ============================================
            
            style.WindowPadding = new Vector2(8, 8);              // 窗口内边距
            style.FramePadding = new Vector2(6, 4);               // 框架内边距（按钮、输入框等）
            style.CellPadding = new Vector2(4, 2);                // 单元格内边距
            style.ItemSpacing = new Vector2(8, 4);                // 项目间距
            style.ItemInnerSpacing = new Vector2(4, 4);            // 项目内部间距
            style.TouchExtraPadding = new Vector2(0, 0);           // 触摸额外内边距
            style.IndentSpacing = 21.0f;                           // 缩进间距
            style.ScrollbarSize = 14.0f;                           // 滚动条大小
            style.GrabMinSize = 10.0f;                             // 抓取器最小尺寸
            
            // ============================================
            // 圆角配置
            // ============================================
            
            style.WindowRounding = 4.0f;                           // 窗口圆角
            style.ChildRounding = 4.0f;                           // 子窗口圆角
            style.FrameRounding = 3.0f;                           // 框架圆角（按钮、输入框等）
            style.PopupRounding = 4.0f;                            // 弹出窗口圆角
            style.ScrollbarRounding = 4.0f;                       // 滚动条圆角
            style.GrabRounding = 3.0f;                             // 抓取器圆角
            style.TabRounding = 4.0f;                              // 标签圆角
            style.LogSliderDeadzone = 4.0f;                        // 对数滑块死区
            
            // ============================================
            // 边框和阴影
            // ============================================
            
            style.WindowBorderSize = 1.0f;                         // 窗口边框大小
            style.ChildBorderSize = 1.0f;                          // 子窗口边框大小
            style.PopupBorderSize = 1.0f;                          // 弹出窗口边框大小
            style.FrameBorderSize = 1.0f;                          // 框架边框大小
            style.TabBorderSize = 1.0f;                             // 标签边框大小
            
            // ============================================
            // 其他视觉效果
            // ============================================
            
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);      // 窗口标题对齐（左对齐，垂直居中）
            style.WindowMenuButtonPosition = ImGuiDir.Right;       // 菜单按钮位置（右侧）
            style.ColorButtonPosition = ImGuiDir.Right;            // 颜色按钮位置（右侧）
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);        // 按钮文本对齐（居中）
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);    // 可选择文本对齐（左上）
            style.DisplaySafeAreaPadding = new Vector2(3, 3);      // 显示安全区内边距
            
            // Alpha 值
            style.Alpha = 1.0f;                                     // 全局透明度
            style.DisabledAlpha = 0.60f;                            // 禁用项透明度
            
            // 反锯齿
            style.AntiAliasedLines = true;                          // 线条反锯齿
            style.AntiAliasedLinesUseTex = true;                    // 使用纹理的线条反锯齿
            style.AntiAliasedFill = true;                           // 填充反锯齿
            
            // 曲线细分
            style.CurveTessellationTol = 1.25f;                     // 曲线细分容差
            
            // 圆角细分
            style.CircleTessellationMaxError = 0.30f;               // 圆形细分最大误差
        }
        
        /// <summary>
        /// 应用自定义缩放（用于高 DPI 显示）
        /// </summary>
        /// <param name="scale">缩放因子（1.0 = 100%）</param>
        public static void ApplyScale(float scale)
        {
            var style = ImGui.GetStyle();
            var io = ImGui.GetIO();
            
            // 缩放字体
            if (io.Fonts.Fonts.Size > 0)
            {
                // 注意：字体缩放需要在 RebuildFontAtlas 时设置
                // 这里只缩放 UI 元素
            }
            
            // 缩放间距和尺寸
            style.WindowPadding *= scale;
            style.FramePadding *= scale;
            style.CellPadding *= scale;
            style.ItemSpacing *= scale;
            style.ItemInnerSpacing *= scale;
            style.TouchExtraPadding *= scale;
            style.IndentSpacing *= scale;
            style.ScrollbarSize *= scale;
            style.GrabMinSize *= scale;
            
            // 缩放圆角（可选，通常保持固定值更美观）
            // style.WindowRounding *= scale;
            // style.ChildRounding *= scale;
            // style.FrameRounding *= scale;
        }
    }
}

