using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Graphics;

namespace Astora.Engine.Components;
public struct SpriteRendererComponent
{
    public Texture2D? Texture;
    public Rectangle? SourceRect;
    public Color      Color;
    public Vector2    Origin;
    public float      SortKey;
    public RenderLayer Layer;
    public SpriteEffects Effects;

    public SpriteRendererComponent(Texture2D? tex, Color? tint = null, RenderLayer layer = RenderLayer.World)
    {
        Texture = tex;
        SourceRect = null;
        Color = tint ?? Color.White;
        Origin = Vector2.Zero;
        SortKey = 0.5f;
        Layer = layer;
        Effects = SpriteEffects.None;
    }
}