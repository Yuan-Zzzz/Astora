using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Astora.Engine.Core;
using Astora.Engine.Graphics;

namespace Astora.Sandbox;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _gdm;
    private IGraphicsService _gfx = null!;
    private ITime _time = null!;

    public Game1()
    {
        _gdm = new GraphicsDeviceManager(this);
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);
        IsMouseVisible = true;
        Window.Title = "Astora Sandbox";
    }

    protected override void LoadContent()
    {
        _gfx = new GraphicsService(GraphicsDevice);
    }

    protected override void Draw(GameTime gameTime)
    {
        _time = new Time(
            (float)gameTime.ElapsedGameTime.TotalSeconds,
            (float)gameTime.TotalGameTime.TotalSeconds);

        _gfx.Draw(_time);
        base.Draw(gameTime);
    }
    

}