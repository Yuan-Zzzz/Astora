using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Core;

namespace Astora.Engine.Graphics;

public abstract class GraphicsScreen
{
    public virtual void Update(ITime t) { }
    public abstract void Draw(ITime t);
}

public interface IGraphicsService
{
    GraphicsDevice Device { get; }
    void PushScreen(GraphicsScreen screen);
    void RemoveScreen(GraphicsScreen screen);
    void Draw(ITime t);
}