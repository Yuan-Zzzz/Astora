using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline.RenderPass;

public class FinalCompositionPass : RenderPass
{
    public FinalCompositionPass() : base("CompositionPass") { }

    public override void Execute(RenderContext context)
    {
        var sourceTexture = context.SourceBuffer;
        if (sourceTexture == null) return;
        Matrix scaleMatrix = Engine.GetScaleMatrix();
        context.RenderBatcher.Begin(scaleMatrix, SamplerState.PointClamp);
        context.RenderBatcher.Draw(
            sourceTexture,
            Vector2.Zero,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            Vector2.One,
            SpriteEffects.None,
            0f
        );

        context.RenderBatcher.End();
    }
}
