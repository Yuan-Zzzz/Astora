using Astora.Core;
using Astora.Core.Time;

namespace Astora.Engine.Core;

public sealed class LogicScheduler
{
    private readonly List<ILogicSystem> _systems = new();

    public void Add(ILogicSystem s)
    {
        _systems.Add(s);
        _systems.Sort((a,b) => a.Order.CompareTo(b.Order));
    }

    public void Remove(ILogicSystem s) => _systems.Remove(s);

    public void Tick(ITime t)
    {
        for (int i = 0; i < _systems.Count; i++)
            _systems[i].TickLogic(t);
    }
    public void Clear() => _systems.Clear();
}