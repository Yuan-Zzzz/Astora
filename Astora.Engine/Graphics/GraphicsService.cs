using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Core;

namespace Astora.Engine.Graphics;

public sealed class GraphicsService(GraphicsDevice device) : IGraphicsService
{
    private readonly List<GraphicsScreen> _screens = new();
    public GraphicsDevice Device { get; } = device;

    public void PushScreen(GraphicsScreen s) => _screens.Add(s);
    public void RemoveScreen(GraphicsScreen s) => _screens.Remove(s);

    public void Draw(ITime t)
    {
        foreach (var s in _screens) s.Update(t);
        foreach (var s in _screens) s.Draw(t);
    }
}