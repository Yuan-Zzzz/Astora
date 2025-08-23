using Astora.ECS;

namespace Astora.Engine.Scene;

public abstract class Scene
{
    public string Name { get; }
    public World World { get; } = new();
    public bool IsLoaded { get; private set; }

    protected Scene(string name = "Untitled") => Name = name;
    
    public abstract void OnLoad(SceneContext ctx);
    
    public abstract void RegisterSystems(SceneContext ctx);
    
    public virtual void OnUnload(SceneContext ctx) { }
    
    public virtual void OnViewportResize(SceneContext ctx, int width, int height) { }

    internal void MarkLoaded(bool value) => IsLoaded = value;
}