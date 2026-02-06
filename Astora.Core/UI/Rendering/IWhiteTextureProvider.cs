using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.UI.Rendering;

/// <summary>
/// Provides a shared 1x1 white texture for UI solid-color quads. Abstraction to keep UI domain independent of engine context.
/// </summary>
public interface IWhiteTextureProvider
{
    /// <summary>
    /// Returns a 1x1 white texture, or null if not available (e.g. device not ready).
    /// </summary>
    Texture2D? GetWhiteTexture();
}
