

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline.RenderPass;

public class SceneRenderPass : RenderPass
{
    public SceneRenderPass() : base("ScenePass") { }

    public override void Execute(RenderContext context)
    {
        var camera = context.ActiveCamera;
        Matrix viewMatrix = camera?.GetViewMatrix() ?? Matrix.Identity;

        // 使用 Batcher 开启
        context.RenderBatcher.Begin(viewMatrix, SamplerState.PointClamp);
        
        context.CurrentScene.Draw(context.RenderBatcher); 

        // 结束
        context.RenderBatcher.End();
    }
}