using Astora.Core.Time;
using Astora.Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Engine.Systems;

public struct ClearScreenSystem : IRenderSystem
{
    private readonly GraphicsDevice _gd;
    public int Order => 590;
    public Color ClearColor { get; set; } = Color.CornflowerBlue;

    public ClearScreenSystem(GraphicsDevice gd) => _gd = gd;

    public void TickRender(ITime t)
    {
        _gd.Clear(ClearColor);
    }
}