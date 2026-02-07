using Astora.Core;
using Astora.Core.Inputs;
using Astora.Core.Scene;
using Astora.SandBox.Application;
using Astora.SandBox.Demos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Astora.SandBox.Scripts;

/// <summary>
/// UI interactive test host: initializes engine, attaches minimal scene, runs current UI demo.
/// Press F2 to cycle through demos. No game logic; only engine loop and demo runner.
/// </summary>
public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly DemoRunner _demoRunner;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        _demoRunner = new DemoRunner(new IUIDemoCase[]
        {
            new LabelFontSizesDemoCase(),
            new LabelButtonDemoCase(),
            new ButtonClickDemoCase(),
            new BoxContainerDemoCase(),
            new MarginContainerDemoCase(),
            new MultipleButtonsDemoCase(),
            new LayeringDemoCase()
        });
    }

    protected override void Initialize()
    {
        base.Initialize();
        Engine.Initialize(Content, _graphics);
        Engine.LoadProjectConfig();
        var sceneRoot = SceneBootstrap.BuildMinimalScene();
        Engine.CurrentScene.AttachScene(sceneRoot);
        _demoRunner.Run(Engine.CurrentScene.Root);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Input.IsKeyPressed(Keys.F1))
            _demoRunner.SwitchToNext(Engine.CurrentScene.Root);
        Engine.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime)
    {
        // 设计分辨率内清屏为白色，窗口黑边保持黑色（由管线在合成前 Clear(Color.Black) 保证）
        Engine.Render(gameTime, Color.White);
        base.Draw(gameTime);
    }

    /// <summary>Sync back buffer to window size so UI and scene scale correctly when resizing.</summary>
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
