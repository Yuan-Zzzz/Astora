using ImGuiNET;
using System.Numerics;

namespace Astora.Editor.UI
{
    /// <summary>
    /// ImGui 样式管理器 - Godot 4 风格黑灰主题
    /// </summary>
    public static class ImGuiStyleManager
    {
        // ============================================
        // Godot 4 黑灰主题色板常量
        // ============================================

        // 背景色系（纯黑灰，无蓝色调）
        private static readonly Vector4 BgDarkest   = HexColor(0x1A1A1A);
        private static readonly Vector4 BgDarker    = HexColor(0x1D1D1D);
        private static readonly Vector4 BgBase      = HexColor(0x232323);
        private static readonly Vector4 BgLighter   = HexColor(0x2B2B2B);
        private static readonly Vector4 BgLightest  = HexColor(0x343434);

        // 表面色
        private static readonly Vector4 Surface     = HexColor(0x3D3D3D);
        private static readonly Vector4 SurfaceHov  = HexColor(0x484848);
        private static readonly Vector4 SurfaceAlt  = HexColor(0x303030);

        // 文本
        private static readonly Vector4 TextPrimary  = HexColor(0xCDCFD2);
        private static readonly Vector4 TextDisabled = HexColor(0x6C6C6C);

        // 强调色（Godot 经典蓝）
        private static readonly Vector4 Accent       = HexColor(0x699CE8);
        private static readonly Vector4 AccentDark   = HexColor(0x2C5B84);
        private static readonly Vector4 AccentHover  = HexColor(0x37699A);
        private static readonly Vector4 AccentDeep   = HexColor(0x245078);

        // 边框/分隔
        private static readonly Vector4 BorderColor  = HexColor(0x1A1A1A);
        private static readonly Vector4 SepColor     = HexColor(0x303030);

        // Tab
        private static readonly Vector4 TabNormal    = HexColor(0x2B2B2B);
        private static readonly Vector4 TabHovered   = HexColor(0x343434);
        private static readonly Vector4 TabActive    = HexColor(0x343434);

        // 滚动条
        private static readonly Vector4 ScrollBg     = HexColor(0x1D1D1D);
        private static readonly Vector4 ScrollGrab   = HexColor(0x484848);
        private static readonly Vector4 ScrollGrabH  = HexColor(0x5A5A5A);
        private static readonly Vector4 ScrollGrabA  = HexColor(0x6A6A6A);

        /// <summary>
        /// 将 0xRRGGBB 转换为 ImGui Vector4 颜色
        /// </summary>
        private static Vector4 HexColor(uint hex, float alpha = 1.0f)
        {
            float r = ((hex >> 16) & 0xFF) / 255f;
            float g = ((hex >> 8) & 0xFF) / 255f;
            float b = (hex & 0xFF) / 255f;
            return new Vector4(r, g, b, alpha);
        }

        /// <summary>
        /// 应用 Godot 4 黑灰深色主题
        /// </summary>
        public static void ApplyModernDarkTheme()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // ============================================
            // 窗口和背景
            // ============================================
            colors[(int)ImGuiCol.WindowBg]          = BgBase;
            colors[(int)ImGuiCol.ChildBg]           = BgDarker;
            colors[(int)ImGuiCol.PopupBg]           = BgLighter;
            colors[(int)ImGuiCol.TitleBg]           = BgDarkest;
            colors[(int)ImGuiCol.TitleBgActive]     = BgLighter;
            colors[(int)ImGuiCol.TitleBgCollapsed]  = BgDarkest;
            colors[(int)ImGuiCol.MenuBarBg]         = BgDarkest;

            // ============================================
            // 文本
            // ============================================
            colors[(int)ImGuiCol.Text]              = TextPrimary;
            colors[(int)ImGuiCol.TextDisabled]      = TextDisabled;

            // ============================================
            // 强调色（选中/高亮）
            // ============================================
            colors[(int)ImGuiCol.Header]            = AccentDark;
            colors[(int)ImGuiCol.HeaderHovered]     = AccentHover;
            colors[(int)ImGuiCol.HeaderActive]      = AccentDeep;

            // ============================================
            // 按钮
            // ============================================
            colors[(int)ImGuiCol.Button]            = Surface;
            colors[(int)ImGuiCol.ButtonHovered]     = SurfaceHov;
            colors[(int)ImGuiCol.ButtonActive]      = AccentDark;

            // ============================================
            // 框架（输入框、下拉框、Checkbox 底色）
            // ============================================
            colors[(int)ImGuiCol.FrameBg]           = BgLightest;
            colors[(int)ImGuiCol.FrameBgHovered]    = SurfaceHov;
            colors[(int)ImGuiCol.FrameBgActive]     = Surface;

            // ============================================
            // 边框
            // ============================================
            colors[(int)ImGuiCol.Border]            = BorderColor;
            colors[(int)ImGuiCol.BorderShadow]      = new Vector4(0, 0, 0, 0);

            // ============================================
            // 分隔线
            // ============================================
            colors[(int)ImGuiCol.Separator]         = SepColor;
            colors[(int)ImGuiCol.SeparatorHovered]  = AccentHover;
            colors[(int)ImGuiCol.SeparatorActive]   = Accent;

            // ============================================
            // 滚动条
            // ============================================
            colors[(int)ImGuiCol.ScrollbarBg]       = ScrollBg;
            colors[(int)ImGuiCol.ScrollbarGrab]     = ScrollGrab;
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = ScrollGrabH;
            colors[(int)ImGuiCol.ScrollbarGrabActive]  = ScrollGrabA;

            // ============================================
            // 滑块 / 复选 / 图表
            // ============================================
            colors[(int)ImGuiCol.SliderGrab]        = Accent;
            colors[(int)ImGuiCol.SliderGrabActive]  = AccentHover;
            colors[(int)ImGuiCol.CheckMark]         = Accent;
            colors[(int)ImGuiCol.PlotLines]         = Accent;
            colors[(int)ImGuiCol.PlotLinesHovered]  = AccentHover;
            colors[(int)ImGuiCol.PlotHistogram]     = Accent;
            colors[(int)ImGuiCol.PlotHistogramHovered] = AccentHover;

            // ============================================
            // Tab
            // ============================================
            colors[(int)ImGuiCol.Tab]               = TabNormal;
            colors[(int)ImGuiCol.TabHovered]        = TabHovered;
            colors[(int)ImGuiCol.TabSelected]       = TabActive;
            colors[(int)ImGuiCol.TabDimmed]         = BgDarkest;
            colors[(int)ImGuiCol.TabDimmedSelected] = BgLighter;

            // ============================================
            // 拖拽
            // ============================================
            colors[(int)ImGuiCol.DragDropTarget]    = new Vector4(Accent.X, Accent.Y, Accent.Z, 0.60f);

            // ============================================
            // 调整大小手柄
            // ============================================
            colors[(int)ImGuiCol.ResizeGrip]        = new Vector4(Surface.X, Surface.Y, Surface.Z, 0.25f);
            colors[(int)ImGuiCol.ResizeGripHovered] = AccentHover;
            colors[(int)ImGuiCol.ResizeGripActive]  = Accent;

            // ============================================
            // Docking
            // ============================================
            colors[(int)ImGuiCol.DockingPreview]    = new Vector4(Accent.X, Accent.Y, Accent.Z, 0.70f);
            colors[(int)ImGuiCol.DockingEmptyBg]    = BgDarker;

            // ============================================
            // 间距和尺寸
            // ============================================
            style.WindowPadding      = new Vector2(8, 6);
            style.FramePadding       = new Vector2(6, 3);
            style.CellPadding        = new Vector2(4, 2);
            style.ItemSpacing        = new Vector2(8, 4);
            style.ItemInnerSpacing   = new Vector2(4, 4);
            style.TouchExtraPadding  = new Vector2(0, 0);
            style.IndentSpacing      = 20.0f;
            style.ScrollbarSize      = 12.0f;
            style.GrabMinSize        = 8.0f;

            // ============================================
            // 圆角（Godot 微圆角）
            // ============================================
            style.WindowRounding     = 3.0f;
            style.ChildRounding      = 3.0f;
            style.FrameRounding      = 2.0f;
            style.PopupRounding      = 3.0f;
            style.ScrollbarRounding  = 3.0f;
            style.GrabRounding       = 2.0f;
            style.TabRounding        = 3.0f;
            style.LogSliderDeadzone  = 4.0f;

            // ============================================
            // 边框大小（Godot 风格：无框架边框）
            // ============================================
            style.WindowBorderSize   = 1.0f;
            style.ChildBorderSize    = 1.0f;
            style.PopupBorderSize    = 1.0f;
            style.FrameBorderSize    = 0.0f;   // Godot 风格：输入框/按钮无边框
            style.TabBorderSize      = 0.0f;

            // ============================================
            // 其他视觉设置
            // ============================================
            style.WindowTitleAlign          = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition  = ImGuiDir.Right;
            style.ColorButtonPosition       = ImGuiDir.Right;
            style.ButtonTextAlign           = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign       = new Vector2(0.0f, 0.0f);
            style.DisplaySafeAreaPadding    = new Vector2(3, 3);

            style.Alpha          = 1.0f;
            style.DisabledAlpha  = 0.50f;

            // 反锯齿
            style.AntiAliasedLines       = true;
            style.AntiAliasedLinesUseTex = true;
            style.AntiAliasedFill        = true;

            style.CurveTessellationTol       = 1.25f;
            style.CircleTessellationMaxError = 0.30f;
        }

        /// <summary>
        /// 应用自定义缩放（用于高 DPI 显示）
        /// </summary>
        /// <param name="scale">缩放因子（1.0 = 100%）</param>
        public static void ApplyScale(float scale)
        {
            if (scale <= 0f || Math.Abs(scale - 1.0f) < 0.001f)
                return;

            var style = ImGui.GetStyle();

            style.WindowPadding     *= scale;
            style.FramePadding      *= scale;
            style.CellPadding       *= scale;
            style.ItemSpacing       *= scale;
            style.ItemInnerSpacing  *= scale;
            style.TouchExtraPadding *= scale;
            style.IndentSpacing     *= scale;
            style.ScrollbarSize     *= scale;
            style.GrabMinSize       *= scale;
        }

        /// <summary>
        /// 获取强调色（供其他 UI 组件使用）
        /// </summary>
        public static Vector4 GetAccentColor() => Accent;

        /// <summary>
        /// 获取暗强调色（供选中状态等使用）
        /// </summary>
        public static Vector4 GetAccentDarkColor() => AccentDark;

        /// <summary>
        /// 获取主要文本颜色
        /// </summary>
        public static Vector4 GetTextColor() => TextPrimary;

        /// <summary>
        /// 获取禁用文本颜色
        /// </summary>
        public static Vector4 GetTextDisabledColor() => TextDisabled;
    }
}
