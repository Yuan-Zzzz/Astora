using Astora.Core.Time;

namespace Astora.Engine.Components;

public sealed class ScriptComponent
{
    public Behaviour? Instance;
    public Func<Behaviour>? Instantiate;
    public Action<ScriptComponent>? Destroy;

    public ScriptComponent(Func<Behaviour> factory)
    {
        Instantiate = factory;
        Destroy = nsc => nsc.Instance?.OnDestroy();
    }
}

public abstract class Behaviour
{
    public int Entity;
    public ECS.World? World;
    public virtual void OnCreate() { }
    public virtual void OnUpdate(ITime t) { }
    public virtual void OnDestroy() { }
}