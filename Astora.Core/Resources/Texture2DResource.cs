namespace Astora.Core.Resources;

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
        return new Texture2DResource(texture, path);
    }
}
