using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Core.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline;

public class RenderContext
{
    public GraphicsDevice GraphicsDevice { get; set; }
    public RenderBatcher RenderBatcher { get; set; }
    public SceneTree CurrentScene { get; set; }
    public Camera2D ActiveCamera { get; set; }
    public GameTime GameTime { get; set; }

    /// <summary>View matrix for world / scene nodes.</summary>
    public Matrix ViewMatrix { get; set; } = Matrix.Identity;

    /// <summary>Transform for UI roots; null = Identity (screen space).</summary>
    public Matrix? UIMatrix { get; set; }

    /// <summary>UI drawing; null = SceneTree will create default.</summary>
    public IWhiteTextureProvider WhiteTextureProvider { get; set; }

    /// <summary>当前渲染的目标纹理（如果为null则是屏幕）</summary>
    public RenderTarget2D DestinationBuffer { get; set; }

    /// <summary>用于传递 Pass 之间的临时纹理（例如后期处理）</summary>
    public RenderTarget2D SourceBuffer { get; set; }
}