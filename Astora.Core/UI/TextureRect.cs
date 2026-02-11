using Astora.Core.Attributes;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.UI;

/// <summary>
/// Displays a texture in a rect (Godot TextureRect style). StretchMode controls how the texture fits the FinalRect.
/// </summary>
public enum StretchMode
{
    /// <summary>Scale to fill the rect (may distort).</summary>
    Scale,
    /// <summary>Scale keeping aspect ratio; fit inside rect (may have letterbox).</summary>
    KeepAspect,
    /// <summary>Scale keeping aspect ratio; cover rect (may crop).</summary>
    KeepAspectCover,
    /// <summary>Center texture at native size (no scale).</summary>
    Center
}

/// <summary>
/// Leaf control that draws a texture. Uses FinalRect; texture source can be full or a region.
/// </summary>
public class TextureRect : Control
{
    private Texture2D? _texture;
    [SerializeField] private Rectangle? _textureRegion;
    [SerializeField] private StretchMode _stretchMode = StretchMode.Scale;

    public Texture2D? Texture
    {
        get => _texture;
        set
        {
            if (_texture == value) return;
            _texture = value;
            InvalidateVisual();
        }
    }

    /// <summary>Optional source rectangle on the texture. When null, uses full texture bounds.</summary>
    public Rectangle? TextureRegion
    {
        get => _textureRegion;
        set => _textureRegion = value;
    }

    public StretchMode StretchMode
    {
        get => _stretchMode;
        set => _stretchMode = value;
    }

    public TextureRect() : base("TextureRect") { }

    public TextureRect(string name) : base(name) { }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        if (!Visible || _texture == null) return;
        var r = FinalRect;
        var src = _textureRegion ?? new Rectangle(0, 0, _texture.Width, _texture.Height);
        float tw = src.Width;
        float th = src.Height;
        Vector2 pos = new Vector2(r.X, r.Y);
        Vector2 scale;
        switch (_stretchMode)
        {
            case StretchMode.Scale:
                scale = new Vector2((float)r.Width / tw, (float)r.Height / th);
                break;
            case StretchMode.KeepAspect:
                {
                    float s = Math.Min((float)r.Width / tw, (float)r.Height / th);
                    scale = new Vector2(s, s);
                    pos.X += (r.Width - tw * s) / 2f;
                    pos.Y += (r.Height - th * s) / 2f;
                }
                break;
            case StretchMode.KeepAspectCover:
                {
                    float s = Math.Max((float)r.Width / tw, (float)r.Height / th);
                    scale = new Vector2(s, s);
                    pos.X += (r.Width - tw * s) / 2f;
                    pos.Y += (r.Height - th * s) / 2f;
                }
                break;
            case StretchMode.Center:
                scale = Vector2.One;
                pos.X += (r.Width - tw) / 2f;
                pos.Y += (r.Height - th) / 2f;
                break;
            default:
                scale = new Vector2((float)r.Width / tw, (float)r.Height / th);
                break;
        }
        renderBatcher.Draw(
            _texture,
            pos,
            src,
            Modulate,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f
        );
    }
}
