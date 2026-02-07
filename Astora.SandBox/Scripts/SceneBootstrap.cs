using Astora.Core.Nodes;

namespace Astora.SandBox.Scripts;

/// <summary>
/// Builds the minimal scene for UI interactive testing: root node and camera only.
/// No file load; scene is constructed in memory.
/// </summary>
public static class SceneBootstrap
{
    /// <summary>Builds a minimal scene root with a Camera2D (required by engine).</summary>
    public static Node BuildMinimalScene()
    {
        var root = new Node("UITestScene");
        var camera = new Camera2D("MainCamera");
        root.AddChild(camera);
        return root;
    }
}
