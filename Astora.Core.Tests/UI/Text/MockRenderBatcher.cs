using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI.Text;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Tests.UI.Text;

/// <summary>No-op implementation of IRenderBatcher for tests that only need Draw to not throw.</summary>
internal class MockRenderBatcher : IRenderBatcher
{
    public void Begin(Matrix transformMatrix, SamplerState? sampler = null) { }
    public void End() { }
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, BlendState? blendState = null, Effect? effect = null) { }
    public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color) { }
    public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color, TextDrawOptions options) { }
    public void DrawRichText(FontStashSharp.RichText.RichTextLayout layout, Vector2 position, Color baseColor, HorizontalAlignment alignment = HorizontalAlignment.Left) { }
    public void PushScissorRect(Rectangle rect) { }
    public void PopScissorRect() { }
}
