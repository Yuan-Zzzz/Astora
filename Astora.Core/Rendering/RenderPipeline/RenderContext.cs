using Astora.Core.Nodes;
using Astora.Core.Renderer;
using Astora.Core.Scene;
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
    
    // 当前渲染的目标纹理（如果为null则是屏幕）
    public RenderTarget2D DestinationBuffer { get; set; }
    
    // 用于传递 Pass 之间的临时纹理（例如后期处理）
    public RenderTarget2D SourceBuffer { get; set; }
}