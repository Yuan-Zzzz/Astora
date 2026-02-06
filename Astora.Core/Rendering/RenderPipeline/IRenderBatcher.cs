using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline;

/// <summary>
/// Abstraction for batching 2D draw calls. Allows nodes and tests to depend on an interface
/// rather than the concrete RenderBatcher.
/// </summary>
public interface IRenderBatcher
{
    void Begin(Matrix transformMatrix, SamplerState? sampler = null);
    void End();
    void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth,
        BlendState? blendState = null, Effect? effect = null);
    /// <summary>Draw text using a sprite font. Used by Label.</summary>
    void DrawString(SpriteFont font, string text, Vector2 position, Color color);
    /// <summary>Push a scissor rectangle (e.g. for Control.ClipContent). Must be paired with PopScissorRect.</summary>
    void PushScissorRect(Rectangle rect);
    /// <summary>Pop the last scissor rectangle.</summary>
    void PopScissorRect();
}
