using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.UI;

/// <summary>
/// Leaf control that draws a solid color rectangle. Uses FinalRect and Modulate; obtains 1x1 white texture from UIDrawContext.
/// </summary>
public class Panel : Control
{
    public Panel() : base("Panel") { }

    public Panel(string name) : base(name) { }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        if (!Visible) return;
        var tex = UIDrawContext.Current?.GetWhiteTexture();
        if (tex == null) return;
        var r = FinalRect;
        renderBatcher.Draw(
            tex,
            new Vector2(r.X, r.Y),
            new Rectangle(0, 0, 1, 1),
            Modulate,
            0f,
            Vector2.Zero,
            new Vector2(r.Width, r.Height),
            SpriteEffects.None,
            0f
        );
    }
}
