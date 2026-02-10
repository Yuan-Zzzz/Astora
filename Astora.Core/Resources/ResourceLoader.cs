using Microsoft.Xna.Framework.Content;

namespace Astora.Core.Resources;

public static class ResourceLoader
{
    private static Dictionary<string, Resource> _resourceCache = new();
    private static Dictionary<Type, IResourceImporter> _importers = new();
    private static ContentManager? _contentManager;

    public static void Initialize(ContentManager contentManager)
    {
        _contentManager = contentManager;
        RegisterDefaultImporter();
    }


    private static void RegisterDefaultImporter()
    {
        RegisterImporter<Texture2DResource>(new Texture2DImporter());
        RegisterImporter<FontResource>(new FontResourceImporter());
    }

    /// <summary>
    /// Registers a resource importer for the given resource type. Call this to support custom resource types.
    /// </summary>
    public static void RegisterImporter<T>(IResourceImporter importer) where T : Resource
    {
        _importers[typeof(T)] = importer;
    }

    public static T Load<T>(string path) where T : Resource
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Resource path cannot be null or empty");
        
        //Check from cache
        if (_resourceCache.TryGetValue(path, out var cachedResource))
        {
            if (cachedResource is T typedResource)
            {
                typedResource.ReferenceCount++;
                return typedResource;
            }
        }

        if (!_importers.TryGetValue(typeof(T), out var importer))
        {
            throw new InvalidOperationException($"No importer registered for type {typeof(T).Name}");
        }
        
        // Resolve relative paths against the content root directory
        var fullpath = path;
        if (!Path.IsPathRooted(fullpath) && _contentManager != null)
        {
            var contentRoot = _contentManager.RootDirectory;
            if (!string.IsNullOrEmpty(contentRoot))
            {
                var resolved = Path.GetFullPath(Path.Combine(contentRoot, fullpath));
                if (File.Exists(resolved))
                    fullpath = resolved;
            }
        }

        Logger.Debug($"Loading resource: {fullpath}");
        var resource = importer.Import(fullpath, _contentManager);
        if (resource == null)
            throw new InvalidOperationException($"Failed to load resource: {path}");
        
        // Cache using original path so callers can look up by the same key
        resource.ResourcePath = path;
        resource.ResourceId = path;
        resource.ReferenceCount = 1;
        resource.IsLoaded = true;
        _resourceCache[fullpath] = resource;
        return (T)resource;
    }

    public static void Release(string path)
    {
        if (_resourceCache.TryGetValue(path, out var resource))
        {
            resource.ReferenceCount--;
            if (resource.ReferenceCount <= 0)
            {
                resource.Dispose();
                _resourceCache.Remove(path);
            }
        }
    }

    public static void Unload(string path)
    {
        if (_resourceCache.TryGetValue(path, out var resource))
        {
            resource.Dispose();
            _resourceCache.Remove(path);
        }

    }

    public static void ClearCache()
    {
        foreach (var resource in _resourceCache.Values)
        {
            resource.Dispose();
        }
        _resourceCache.Clear();
    }

    public static bool Exists(string path)
    {
        return _resourceCache.ContainsKey(path);
    }

    public static void PreLoad<T>(string path) where T : Resource
    {
        if (!Exists(path))
        {
            Load<T>(path);
        }
    }
}
