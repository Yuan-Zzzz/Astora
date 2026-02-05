namespace Astora.Core.Resources;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

public class Texture2DResource : Resource
{
    public Texture2D Texture { get; private set; }

    public int Width => Texture?.Width ?? 0;
    public int Height => Texture?.Height ?? 0;

    internal Texture2DResource() { }

    public Texture2DResource(Texture2D texture, string path = "")
        : base(path)
    {
        Texture = texture;
        IsLoaded = true;
    }

    public override Resource Duplicate(bool deep = false)
    {
        return new Texture2DResource(Texture, ResourcePath);
    }

    public override void Dispose()
    {
        if (Texture != null && !Texture.IsDisposed)
        {
            Texture.Dispose();
        }
        base.Dispose();
    }
}

public class Texture2DImporter : IResourceImporter
{
    public string[] SupportedExtensions => new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    public Resource Import(string path, ContentManager? contentManager)
    {
        var graphicsDevice = Engine.GDM.GraphicsDevice;

        string fullPath = path;
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Texture file not found: {fullPath}");

        Texture2D texture;        
        try
        {
            using (var fileStream = File.OpenRead(fullPath))
            {
                texture = Texture2D.FromStream(graphicsDevice, fileStream);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load texture from file: {fullPath}", ex);
        }

        // Premultiply alpha for correct blending with BlendState.AlphaBlend
        PremultiplyAlpha(texture);

        return new Texture2DResource(texture, path);
    }

    /// <summary>
    /// Converts texture from straight alpha to premultiplied alpha.
    /// This ensures correct blending when using BlendState.AlphaBlend.
    /// </summary>
    private static void PremultiplyAlpha(Texture2D texture)
    {
        var pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);

        for (int i = 0; i < pixels.Length; i++)
        {
            var c = pixels[i];
            if (c.A < 255)
            {
                pixels[i] = Color.FromNonPremultiplied(c.R, c.G, c.B, c.A);
            }
        }

        texture.SetData(pixels);
    }
}
