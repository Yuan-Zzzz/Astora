using Astora.Core.Time;

namespace Astora.Engine.Core;

public sealed class RenderScheduler
{
    private readonly List<IRenderSystem> _systems = new();

    public void Add(IRenderSystem s)
    {
        _systems.Add(s);
        _systems.Sort((a,b) => a.Order.CompareTo(b.Order));
    }

    public void Remove(IRenderSystem s) => _systems.Remove(s);

    public void Tick(ITime t)
    {
        for (int i = 0; i < _systems.Count; i++)
            _systems[i].TickRender(t);
    }
    public void Clear() => _systems.Clear();
}