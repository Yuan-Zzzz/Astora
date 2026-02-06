using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.UI.Rendering;

/// <summary>
/// Creates and caches a 1x1 white texture. Owned by the render pass or pipeline, not by engine context.
/// </summary>
public sealed class WhiteTextureProvider : IWhiteTextureProvider
{
    private readonly GraphicsDevice _device;
    private Texture2D? _texture;

    public WhiteTextureProvider(GraphicsDevice device)
    {
        _device = device ?? throw new ArgumentNullException(nameof(device));
    }

    public Texture2D? GetWhiteTexture()
    {
        if (_device.IsDisposed) return null;
        if (_texture != null && !_texture.IsDisposed) return _texture;
        _texture = new Texture2D(_device, 1, 1);
        _texture.SetData(new[] { Color.White });
        return _texture;
    }
}
