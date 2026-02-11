using Astora.Core;
using Astora.Core.Game;
using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Astora.Core.Project;
using Astora.Core.Scene;
using Astora.SandBox.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Astora.SandBox.Scripts;

/// <summary>
/// SandBox game logic: cycles through IScene demos. Press F1 to cycle. Implements IGameRuntime so the same logic runs standalone and in-editor.
/// </summary>
public class SandBoxGameRuntime : IGameRuntime
{
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

    private SceneTree? _sceneTree;
    private int _sceneIndex;

    public void Initialize(ContentManager content, GameProjectConfig config, SceneTree sceneTree, bool skipInitialSceneLoad)
    {
        _sceneTree = sceneTree;
        if (!skipInitialSceneLoad)
        {
            _sceneIndex = 0;
            LoadCurrentScene();
        }
    }

    public void Update(GameTime gameTime)
    {
        if (_sceneTree == null) return;

        if (Input.IsKeyPressed(Keys.F1))
        {
            _sceneIndex = (_sceneIndex + 1) % _scenes.Length;
            LoadCurrentScene();
        }
    }

    private void LoadCurrentScene()
    {
        if (_sceneTree == null) return;
        var sceneRoot = _scenes[_sceneIndex]();
        _sceneTree.AttachScene(sceneRoot);
    }
}
