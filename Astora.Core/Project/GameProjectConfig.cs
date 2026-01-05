using YamlDotNet.Serialization;

namespace Astora.Core.Project
{
    /// <summary>
    /// 缩放模式
    /// </summary>
    public enum ScalingMode
    {
        /// <summary>
        /// 不缩放，1:1显示
        /// </summary>
        None,
        
        /// <summary>
        /// 保持宽高比，完整显示（可能有黑边）
        /// </summary>
        Fit,
        
        /// <summary>
        /// 保持宽高比，填满屏幕（可能裁剪）
        /// </summary>
        Fill,
        
        /// <summary>
        /// 拉伸填满屏幕（可能变形）
        /// </summary>
        Stretch,
        
        /// <summary>
        /// 像素完美缩放（整数倍缩放）
        /// </summary>
        PixelPerfect
    }

    /// <summary>
    /// 游戏项目配置 - 存储项目级别的设置
    /// </summary>
    public class GameProjectConfig
    {
        /// <summary>
        /// 设计分辨率宽度
        /// </summary>
        [YamlMember(Alias = "designWidth")]
        public int DesignWidth { get; set; } = 1920;

        /// <summary>
        /// 设计分辨率高度
        /// </summary>
        [YamlMember(Alias = "designHeight")]
        public int DesignHeight { get; set; } = 1080;

        /// <summary>
        /// 缩放模式
        /// </summary>
        [YamlMember(Alias = "scalingMode")]
        public ScalingMode ScalingMode { get; set; } = ScalingMode.Fit;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static GameProjectConfig CreateDefault()
        {
            return new GameProjectConfig
            {
                DesignWidth = 1920,
                DesignHeight = 1080,
                ScalingMode = ScalingMode.Fit
            };
        }
    }
}

