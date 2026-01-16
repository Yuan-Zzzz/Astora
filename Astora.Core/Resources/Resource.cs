namespace Astora.Core.Resources;

public abstract class Resource: IDisposable
{
    public string ResourcePath {get; set;} = string.Empty;
    public string ResourceId {get; set;} = string.Empty;
    internal int ReferenceCount {get; set;} = 0;
    public bool IsLoaded {get; set;} = false;

    protected Resource () {}

    protected Resource(string resourcePath)
    {
        ResourcePath = resourcePath;
        ResourceId = resourcePath;
    }

    public virtual void Dispose()
    {
        IsLoaded = false;
    }

    public abstract Resource Duplicate(bool deep = false);
}
