using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI;
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
        var scene = context.CurrentScene;
        var root = scene?.Root;
        if (root == null || scene == null) return;

        _whiteTextureProvider ??= new WhiteTextureProvider(context.GraphicsDevice);

        var camera = context.ActiveCamera;
        var viewMatrix = camera?.GetViewMatrix() ?? Matrix.Identity;

        context.RenderBatcher.Begin(viewMatrix, SamplerState.PointClamp);
        DrawSceneRecursive(root, context.RenderBatcher);
        context.RenderBatcher.End();

        foreach (var (_, controlRoot) in scene.GetUIRoots())
        {
            UIDrawContext.SetCurrent(_whiteTextureProvider);
            try
            {
                context.RenderBatcher.Begin(Matrix.Identity, SamplerState.PointClamp);
                controlRoot.InternalDraw(context.RenderBatcher);
                context.RenderBatcher.End();
            }
            finally
            {
                UIDrawContext.SetCurrent(null);
            }
        }
    }

    private static void DrawSceneRecursive(Node node, IRenderBatcher batcher)
    {
        if (node is CanvasLayer) return;
        if (node is Control c && c.Parent is not Control) return;
        node.Draw(batcher);
        foreach (var child in node.Children)
            DrawSceneRecursive(child, batcher);
    }
}