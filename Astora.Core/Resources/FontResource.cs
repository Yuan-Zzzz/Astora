using FontStashSharp;
using Microsoft.Xna.Framework.Content;

namespace Astora.Core.Resources;

public class FontResource : Resource
{
    public FontSystem FontSystem { get; private set; }

    internal FontResource() { }

    public FontResource(FontSystem fontSystem, string path = "")
        : base(path)
    {
        FontSystem = fontSystem;
        IsLoaded = true;
    }

    /// <summary>
    /// Returns a drawable font at the specified size (pixels).
    /// FontStashSharp generates glyphs on demand and caches them in an internal atlas.
    /// </summary>
    public SpriteFontBase GetFont(float size)
    {
        return FontSystem.GetFont(size);
    }

    public override void Dispose()
    {
        if (FontSystem != null)
        {
            FontSystem.Dispose();
            FontSystem = null!;
        }
        base.Dispose();
    }

    public override Resource Duplicate(bool deep = false)
    {
        // FontSystem is shared; callers that need isolation should create a new FontResource.
        return new FontResource(FontSystem, ResourcePath);
    }
}

public class FontResourceImporter : IResourceImporter
{
    public string[] SupportedExtensions => new[] { ".ttf", ".otf", ".ttc" };

    public Resource Import(string path, ContentManager? contentManager)
    {
        string fullPath = path;
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Font file not found: {fullPath}");

        var fontSystem = new FontSystem();
        fontSystem.AddFont(File.ReadAllBytes(fullPath));
        return new FontResource(fontSystem, path);
    }
}
