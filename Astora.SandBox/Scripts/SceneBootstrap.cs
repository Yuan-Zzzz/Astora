using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;

namespace Astora.SandBox.Scripts;

/// <summary>
/// Loads or builds the scene for Sandbox. Tries file path first, then fallback path, then minimal scene.
/// </summary>
public static class SceneBootstrap
{
    public static Node LoadOrBuildScene()
    {
        var path = SampleScene.ScenePath;
        if (File.Exists(path))
            return Engine.Serializer.Load(path);

        var altPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Scenes", "Test666.scene");
        if (File.Exists(altPath))
            return Engine.Serializer.Load(Path.GetFullPath(altPath));

        return BuildMinimalScene();
    }

    public static Node BuildMinimalScene()
    {
        var root = new Node("DemoScene");
        var camera = new Camera2D("MainCamera");
        root.AddChild(camera);
        return root;
    }
}
