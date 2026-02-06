using YamlDotNet.Serialization;

namespace Astora.Core.Project;

/// <summary>
/// Scaling modes for adapting the game to different screen sizes
/// </summary>
public enum ScalingMode
{
    /// <summary>
    /// No scaling applied
    /// </summary>
    None,
    
    /// <summary>
    /// Fit the game within the screen while maintaining aspect ratio
    /// </summary>
    Fit,
    
    /// <summary>
    /// Fill the screen, possibly cropping content
    /// </summary>
    Fill,
    
    /// <summary>
    /// Stretch to fill the screen, ignoring aspect ratio
    /// </summary>
    Stretch,
    
    /// <summary>
    /// Scale in whole pixel increments for pixel-perfect rendering
    /// </summary>
    PixelPerfect
}

/// <summary>
/// Game project configuration settings
/// </summary>
public class GameProjectConfig
{
    /// <summary>
    /// Design width of the game
    /// </summary>
    [YamlMember(Alias = "designWidth")]
    public int DesignWidth { get; set; } = 1920;

    /// <summary>
    /// Design height of the game
    /// </summary>
    [YamlMember(Alias = "designHeight")]
    public int DesignHeight { get; set; } = 1080;

    /// <summary>
    /// Scaling mode for adapting to different screen sizes
    /// </summary>
    [YamlMember(Alias = "scalingMode")]
    public ScalingMode ScalingMode { get; set; } = ScalingMode.Fit;
   
    /// <summary>
    /// ContentRootDirectory
    /// </summary>
    [YamlMember(Alias = "contentRootDirectory")]
    public string ContentRootDirectory { get; set; } = "Content";

    /// <summary>
    /// Creates a default game project configuration
    /// </summary>
    public static GameProjectConfig CreateDefault()
    {
        return new GameProjectConfig
        {
            DesignWidth = 1920,
            DesignHeight = 1080,
            ScalingMode = ScalingMode.Fit,
            ContentRootDirectory = "Content"
        };
    }
}
