using Astora.Core;
using Astora.Core.Game;
using Astora.Core.Project;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.SandBox.Scripts;

/// <summary>
/// Host Game: initializes engine and drives SandBoxGameRuntime. Same runtime runs in-editor when playing.
/// </summary>
public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private IGameRuntime _runtime = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        base.Initialize();
        Engine.Initialize(Content, _graphics);
        var config = Engine.LoadProjectConfig() ?? GameProjectConfig.CreateDefault();
        _runtime = new SandBoxGameRuntime();
        _runtime.Initialize(Engine.Content, config, Engine.CurrentScene, skipInitialSceneLoad: false);
    }

    protected override void Update(GameTime gameTime)
    {
        _runtime.Update(gameTime);
        Engine.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime)
    {
        Engine.Render(gameTime, Color.White);
        base.Draw(gameTime);
    }

    private void OnClientSizeChanged(object? sender, EventArgs e)
    {
        if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
        {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        }
    }
}
