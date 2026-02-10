using Astora.Core;
using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.SandBox.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Astora.SandBox.Scripts;

/// <summary>
/// UI interactive test host: initializes engine and cycles through IScene demos.
/// Press F1 to cycle through scenes.
/// </summary>
public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;

    /// <summary>All demo scenes registered as IScene.Build delegates.</summary>
    private readonly Func<Node>[] _scenes =
    {
        SampleScene.Build,
        LabelFontSizesScene.Build,
        LabelButtonScene.Build,
        LabelEffectsScene.Build,
        ButtonClickScene.Build,
        MultipleButtonsScene.Build,
        BoxContainerScene.Build,
        MarginContainerScene.Build,
        LayeringScene.Build,
    };

    private int _sceneIndex;

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
        Engine.LoadProjectConfig();
        LoadCurrentScene();
    }

    protected override void Update(GameTime gameTime)
    {
        if (Input.IsKeyPressed(Keys.F1))
        {
            _sceneIndex = (_sceneIndex + 1) % _scenes.Length;
            LoadCurrentScene();
        }
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

    private void LoadCurrentScene()
    {
        var sceneRoot = _scenes[_sceneIndex]();
        Engine.CurrentScene.AttachScene(sceneRoot);
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
