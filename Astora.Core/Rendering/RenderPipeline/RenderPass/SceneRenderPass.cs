using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline.RenderPass;

public class SceneRenderPass : RenderPass
{
    private IWhiteTextureProvider? _whiteTextureProvider;

    public SceneRenderPass() : base("ScenePass") { }

    public override void Execute(RenderContext context)
    {
        if (context.CurrentScene == null) return;

        _whiteTextureProvider ??= new WhiteTextureProvider(context.GraphicsDevice);
        context.ViewMatrix = context.ActiveCamera?.GetViewMatrix() ?? Matrix.Identity;
        context.UIMatrix = null;
        context.WhiteTextureProvider = _whiteTextureProvider;
        context.CurrentScene.Draw(context);
    }
}