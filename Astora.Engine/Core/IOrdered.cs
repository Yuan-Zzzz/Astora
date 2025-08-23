using Astora.Core.Time;

namespace Astora.Engine.Core;

public interface IOrdered { int Order { get; } }

public interface ILogicSystem : IOrdered
{
    void TickLogic(ITime t);
}

public interface IRenderSystem : IOrdered
{
    void TickRender(ITime t);
}