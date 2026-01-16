using Microsoft.Xna.Framework.Content;

namespace Astora.Core.Resources;

public interface IResourceImporter
{
    Resource Import(string path, ContentManager contentManager);
    
    string[] SupportedExtensions{get; }
} 
