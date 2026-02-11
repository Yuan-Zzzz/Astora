using Astora.Core.Project;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Astora.Core.Game;

/// <summary>
/// Game logic entry point that can be driven by any host (standalone Game or in-editor play).
/// The host is responsible for calling Engine.Update and Engine.Render; the runtime handles
/// input, scene switching, and other game-specific logic.
/// </summary>
public interface IGameRuntime
{
    /// <summary>
    /// Initialize the runtime with the given content, config, and scene tree.
    /// </summary>
    /// <param name="content">Content manager for loading assets.</param>
    /// <param name="config">Project configuration (design resolution, etc.).</param>
    /// <param name="sceneTree">The scene tree to use. The host owns this instance.</param>
    /// <param name="skipInitialSceneLoad">If true, do not replace the current scene (host has already set it). If false, the runtime may load its default/first scene.</param>
    void Initialize(ContentManager content, GameProjectConfig config, SceneTree sceneTree, bool skipInitialSceneLoad);

    /// <summary>
    /// Run one frame of game logic (input, scene changes, etc.). The host will call Engine.Update after this.
    /// </summary>
    void Update(GameTime gameTime);
}
